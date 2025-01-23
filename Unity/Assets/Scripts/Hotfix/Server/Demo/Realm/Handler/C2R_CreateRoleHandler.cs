using System.Collections.Generic;

namespace ET.Server
{
    [MessageSessionHandler(SceneType.Realm)]
    [FriendOfAttribute(typeof(ET.RoleInfo))]
    public class C2R_CreateRoleHandler : MessageSessionHandler<C2R_CreateRole, R2C_CreateRole>
    {
        protected override async ETTask Run(Session session, C2R_CreateRole request, R2C_CreateRole response)
        {
            if (session.GetComponent<SessionLockingComponent>() != null)
            {
                response.Error = ErrorCode.ERR_RequestRepeatedly;
                session.Disconnect().Coroutine();
                return;
            }

            string token = session.Root().GetComponent<TokenComponent>().Get(request.Account);
            if (string.IsNullOrEmpty(token) || token != request.Token)
            {
                response.Error = ErrorCode.ERR_TokenInvalid;
                session.Disconnect().Coroutine();
                return;
            }

            if (string.IsNullOrEmpty(request.RoleName))
            {
                response.Error = ErrorCode.ERR_RoleNameInvalid;
                return;
            }

            CoroutineLockComponent coroutineLockComponent = session.Root().GetComponent<CoroutineLockComponent>();
            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await coroutineLockComponent.Wait(CoroutineLockType.CreateRole, request.Account.GetLongHashCode()))
                {
                    DBComponent dbComponent = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
                    List<RoleInfo> roleInfos = await dbComponent.Query<RoleInfo>(d => d.RoleName == request.RoleName);
                    if (roleInfos != null && roleInfos.Count > 0)
                    {
                        response.Error = ErrorCode.ERR_RoleNameRepeatedly;
                        return;
                    }

                    RoleInfo roleInfo = new RoleInfo();
                    roleInfo.Account = request.Account;
                    roleInfo.RoleName = request.RoleName;
                    roleInfo.ServerId = request.ServerId;
                    roleInfo.State = (int)RoleInfoState.Normal;
                    roleInfo.CreateTime = TimeInfo.Instance.ServerNow();
                    roleInfo.LastLoginTime = 0;
                    await dbComponent.Save<RoleInfo>(roleInfo);

                    response.RoleInfo = roleInfo.ToMessage();

                    roleInfo?.Dispose();
                }
            }
        }
    }

}