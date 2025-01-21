namespace ET.Client
{
    public static class LoginHelper
    {
        public static async ETTask Login(Scene root, string account, string password)
        {
            root.RemoveComponent<ClientSenderComponent>();
            
            ClientSenderComponent clientSenderComponent = root.AddComponent<ClientSenderComponent>();
            
            NetClient2Main_Login response = await clientSenderComponent.LoginAsync(account, password);
            if (response.Error != ErrorCode.ERR_Success)
            {
                Log.Error($"请求登录失败，ERROR: {response.Error}");
                return;
            }
            
            Log.Debug($"请求登录成功 Token:{response.Token}");

            root.GetComponent<PlayerComponent>().Token = response.Token;
            
            // 获取服务器列表
            C2R_GetServerInfos c2RGetServerInfos = C2R_GetServerInfos.Create();
            c2RGetServerInfos.Account = account;
            c2RGetServerInfos.Token = response.Token;
            R2C_GetServerInfos r2CGetServerInfos = await clientSenderComponent.Call(c2RGetServerInfos) as R2C_GetServerInfos;
            if (r2CGetServerInfos.Error != ErrorCode.ERR_Success)
            {
                Log.Error($"获取服务器列表失败 Error:{r2CGetServerInfos.Error}");
                return;
            }

            foreach (var serverInfo in r2CGetServerInfos.ServerInfoList)
            {
                Log.Debug($"serverInfo:{serverInfo}");
            }
            
            
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
        }
    }
}