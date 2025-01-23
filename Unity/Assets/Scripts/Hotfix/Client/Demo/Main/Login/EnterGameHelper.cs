using System;


namespace ET.Client
{
    public static partial class EnterGameHelper
    {
        public static async ETTask EnterMapAsync(Scene root)
        {
            try
            {
                G2C_EnterGame g2CEnterGame = await root.GetComponent<ClientSenderComponent>().Call(C2G_EnterGame.Create()) as G2C_EnterGame;
                
                // 等待场景切换完成
                await root.GetComponent<ObjectWait>().Wait<Wait_SceneChangeFinish>();
                
                EventSystem.Instance.Publish(root, new EnterMapFinish());
            }
            catch (Exception e)
            {
                Log.Error(e);
            }	
        }
        
        public static async ETTask Match(Fiber fiber)
        {
            try
            {
                G2C_Match g2CEnterMap = await fiber.Root.GetComponent<ClientSenderComponent>().Call(C2G_Match.Create()) as G2C_Match;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }	
        }
    }
}