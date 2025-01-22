using NativeCollection.UnsafeType;

namespace ET.Server
{
    [MessageSessionHandler(SceneType.Realm)]
    public class C2R_GetRolesHandler : MessageSessionHandler<C2R_GetRoles, R2C_GetRoles>
    {
        protected override ETTask Run(Session session, C2R_GetRoles request, R2C_GetRoles response)
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
                using (await coroutineLockComponent.Wait(CoroutineLockType.CreateRole, request.Account.GetLongHashCode()))
                {
                    DBComponent dbComponent = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
                    List<RoleInfo> roleInfos = await dbComponent.Query<RoleInfo>(d =>
                            d.Account == request.Account && d.ServerId == request.ServerId && d.State == (int)RoleInfoState.Normal);
                    
                    if (roleInfos != null && roleInfos.Count > 0)
                    {
                        foreach (var roleInfo in roleInfos)
                        {
                            response.RoleInfoList.add(roleInfo.ToMessage());
                            roleInfo?.Dispose();
                        }
                        roleInfos.Clear();
                    }
                }
            }
        }
    }

}