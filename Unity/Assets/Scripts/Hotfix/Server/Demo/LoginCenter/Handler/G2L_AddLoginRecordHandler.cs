namespace ET.Server
{

    [MessageHandler(SceneType.LoginCenter)]
    public class G2L_AddLoginRecordHandler : MessageHandler<Scene, G2L_AddLoginRecord, L2G_AddLoginRecord>
    {
        protected override async ETTask Run(Scene scene, G2L_AddLoginRecord request, L2G_AddLoginRecord response)
        {
            scene.GetComponent<LoginInfoManagerComponent>().Remove(request.Account.GetLongHashCode());
            scene.GetComponent<LoginInfoManagerComponent>().Add(request.Account.GetLongHashCode(), request.ServerId);
            
            await ETTask.CompletedTask;
        }
    }
    
}