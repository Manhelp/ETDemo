using System.Collections.Generic;

namespace ET.Server
{
    [MessageHandler(SceneType.Realm)]
    [FriendOfAttribute(typeof(ET.RoleInfo))]
    public class C2R_DeleteRoleHandler : MessageSessionHandler<C2R_DeleteRole, R2C_DeleteRole>
    {
        protected override async ETTask Run(Session session, C2R_DeleteRole request, R2C_DeleteRole response)
        {
            await ETTask.CompletedTask;
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

            if (string.IsNullOrEmpty(request.Account))
            {
                response.Error = ErrorCode.ERR_LoginAccountInvalid;
                return;
            }

            if (request.RoleInfoId == 0)
            {
                response.Error = ErrorCode.ERR_DeleteRoleIdInvalid;
                return;
            }

            CoroutineLockComponent coroutineLockComponent = session.Root().GetComponent<CoroutineLockComponent>();
            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await coroutineLockComponent.Wait(CoroutineLockType.CreateRole, request.Account.GetLongHashCode()))
                {
                    DBComponent dbComponent = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
                    List<RoleInfo> roleInfos = await dbComponent.Query<RoleInfo>(d =>
                            d.Account == request.Account && d.ServerId == request.ServerId && d.Id == request.RoleInfoId);

                    if (roleInfos == null || roleInfos.Count <= 0)
                    {
                        response.Error = ErrorCode.ERR_DeleteRoleFailure;
                        return;
                    }

                    RoleInfo deleteRoleInfo = roleInfos[0];
                    session.AddChild(deleteRoleInfo);

                    deleteRoleInfo.State = (int)RoleInfoState.Freeze;
                    await dbComponent.Save<RoleInfo>(deleteRoleInfo);

                    response.RoleInfoId = deleteRoleInfo.Id;

                    deleteRoleInfo?.Dispose();
                    roleInfos.Clear();
                }
            }
        }
    }

}