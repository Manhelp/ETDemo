namespace ET.Server
{
    [MessageSessionHandler(SceneType.Realm)]
    public class C2R_GetRealmKeyHandler : MessageSessionHandler<C2R_GetRealmKey, R2C_GetRealmKey>
    {
        protected override async ETTask Run(Session session, C2R_GetRealmKey request, R2C_GetRealmKey response)
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

            CoroutineLockComponent coroutineLockComponent = session.Root().GetComponent<CoroutineLockComponent>();
            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await coroutineLockComponent.Wait(CoroutineLockType.LoginAccount, request.Account.GetLongHashCode()))
                {
                    // 分配一下Gate
                    StartSceneConfig gateConfig = RealmGateAddressHelper.GetGate(request.ServerId, request.Account);
                    
                    // 向Gate请求一下Key， 客户端可以拿这个Key去连我们的Gate
                    R2G_GetLoginKey r2GGetLoginKey = R2G_GetLoginKey.Create();
                    r2GGetLoginKey.Account = request.Account;
                    G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey) await session.Root().GetComponent<MessageSender>().Call(gateConfig.ActorId, r2GGetLoginKey);
                    if (g2RGetLoginKey.Error != ErrorCode.ERR_Success)
                    {
                        response.Error = g2RGetLoginKey.Error;
                        session.Disconnect().Coroutine();
                        return;
                    }

                    response.Address = gateConfig.InnerIPPort.ToString();
                    response.Key = g2RGetLoginKey.Key;
                    response.GateId = g2RGetLoginKey.GateId;
        
                    session.Disconnect().Coroutine();
                }
            }
        }
    }

}