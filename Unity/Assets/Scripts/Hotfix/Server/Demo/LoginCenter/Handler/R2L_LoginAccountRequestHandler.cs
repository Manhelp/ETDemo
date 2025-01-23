namespace ET.Server
{
    [MessageHandler(SceneType.LoginCenter)]
    public class R2L_LoginAccountRequestHandler : MessageHandler<Scene, R2L_LoginAccountRequest, L2R_LoginAccountResponse>
    {
        protected override async ETTask Run(Scene scene, R2L_LoginAccountRequest request, L2R_LoginAccountResponse response)
        {
            long hashcode = request.Account.GetLongHashCode();

            CoroutineLockComponent coroutineLockComponent = scene.GetComponent<CoroutineLockComponent>();
            using (await coroutineLockComponent.Wait(CoroutineLockType.LoginInfoManager, hashcode))
            {
                if (scene.GetComponent<LoginInfoManagerComponent>().IsExist(hashcode))
                {
                    return;
                }
                
                int zoneId = scene.GetComponent<LoginInfoManagerComponent>().Get(hashcode);
                StartSceneConfig gateConfig = RealmGateAddressHelper.GetGate(zoneId, request.Account);
                L2G_DisconnectGameUnit l2GDisconnectGameUnit = L2G_DisconnectGameUnit.Create();
                l2GDisconnectGameUnit.Account = request.Account;
                G2L_DisconnectGameUnit g2LDisconnectGameUnit =
                        (G2L_DisconnectGameUnit) await scene.GetComponent<MessageSender>().Call(gateConfig.ActorId, l2GDisconnectGameUnit);
                
                response.Error = g2LDisconnectGameUnit.Error;
            }
        }
    }

}