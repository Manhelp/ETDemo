using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ET.Server
{
    [MessageSessionHandler(SceneType.Realm)]
    [FriendOf(typeof(Account))]
    public class C2R_LoginAccountHandler : MessageSessionHandler<C2R_LoginAccount, R2C_LoginAccount>
    {
        protected override async ETTask Run(Session session, C2R_LoginAccount request, R2C_LoginAccount response)
        {
            session.RemoveComponent<SessionAcceptTimeoutComponent>();

            if (session.GetComponent<SessionLockingComponent>() != null)
            {
                response.Error = ErrorCode.ERR_RequestRepeatedly;
                session.Disconnect().Coroutine();
                return;
            }

            if (string.IsNullOrEmpty(request.Account) || string.IsNullOrEmpty(request.Password))
            {
                response.Error = ErrorCode.ERR_LoginInfoIsNull;
                session.Disconnect().Coroutine();
                return;
            }
            
            if(!Regex.IsMatch(request.Account.Trim(), @"^(?=.*[0-9].*)(?=.*[A-Z].*)(?=.*[a-z].*).{6,15}$"))
            {
                response.Error = ErrorCode.ERR_LoginAccountInvalid;
                session.Disconnect().Coroutine();
                return;
            }
            
            if(!Regex.IsMatch(request.Password.Trim(), @"^[A-Za-z0-9]+$"))
            {
                response.Error = ErrorCode.ERR_LoginPasswordInvalid;
                session.Disconnect().Coroutine();
                return;
            }
            
            CoroutineLockComponent coroutineLockComponent = session.Root().GetComponent<CoroutineLockComponent>();
            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await coroutineLockComponent.Wait(CoroutineLockType.LoginAccount, request.Account.GetLongHashCode()))
                {
                    DBComponent dbComponent = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
                    List<Account> accountList = await dbComponent.Query<Account>(data => data.AccountName.Equals(request.Account));
                    Account account = null;
                    if (accountList is { Count: > 0 })
                    {
                        account = accountList[0];
                        session.AddChild(account);
                        if (account.AccountType == (int)AccountType.BlackList)
                        {
                            response.Error = ErrorCode.ERR_AccountInBlackList;
                            session.Disconnect().Coroutine();
                            account?.Dispose();
                            return;
                        }

                        if (!account.Password.Equals(request.Password))
                        {
                            response.Error = ErrorCode.ERR_LoginPasswordError;
                            session.Disconnect().Coroutine();
                            account?.Dispose();
                            return;
                        }
                    }
                    else
                    {
                        account = session.AddChild<Account>();
                        account.AccountName = request.Account;
                        account.Password = request.Password;
                        account.CreateTime = TimeInfo.Instance.ServerNow();
                        account.AccountType = (int)AccountType.General;
                        
                        await dbComponent.Save<Account>(account);
                    }
                    
                    R2L_LoginAccountRequest rLLoginAccountRequest = R2L_LoginAccountRequest.Create();
                    rLLoginAccountRequest.Account = request.Account;

                    StartSceneConfig loginCenterConfig = StartSceneConfigCategory.Instance.LoginCenterConfig;
                    L2R_LoginAccountResponse rLLoginAccountResponse = (L2R_LoginAccountResponse)await session.Fiber().Root
                            .GetComponent<MessageSender>().Call(loginCenterConfig.ActorId, rLLoginAccountRequest);

                    if (rLLoginAccountResponse.Error != ErrorCode.ERR_Success)
                    {
                        response.Error = rLLoginAccountResponse.Error;
                        session?.Disconnect().Coroutine();
                        account?.Dispose();
                        return;
                    }

                    Session otherSession = session.Root().GetComponent<AccountSessionComponent>().Get(request.Account);
                    otherSession?.Send(A2C_Disconnect.Create());
                    otherSession?.Disconnect().Coroutine();
                    
                    session.Root().GetComponent<AccountSessionComponent>().Add(request.Account, session);
                    session.AddComponent<AccountCheckoutTimeComponent, string>(request.Account);
                    
                    string token = TimeInfo.Instance.ServerNow().ToString() + RandomGenerator.RandomNumber(int.MinValue, int.MaxValue);
                    session.Root().GetComponent<TokenComponent>().Remove(request.Account);
                    session.Root().GetComponent<TokenComponent>().Add(request.Account, token);
                    
                    response.Token = token;
                    
                    account?.Dispose();
                }
            }
            
        }
    }
}
