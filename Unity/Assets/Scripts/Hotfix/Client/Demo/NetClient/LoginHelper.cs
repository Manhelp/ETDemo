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
            
            ServerInfoProto serverInfoProto = r2CGetServerInfos.ServerInfoList[0];
            
            // 获取角色
            C2R_GetRoles c2RGetRoles = C2R_GetRoles.Create();
            c2RGetRoles.Account = account;
            c2RGetRoles.Token = response.Token;
            c2RGetRoles.ServerId = serverInfoProto.Id;
            R2C_GetRoles r2CGetRoles = await clientSenderComponent.Call(c2RGetRoles) as R2C_GetRoles;
            if (r2CGetRoles.Error != ErrorCode.ERR_Success)
            {
                Log.Error($"获取角色 区服：{c2RGetRoles.ServerId} account:{account} Error:{r2CGetRoles.Error}");
                return;
            }

            RoleInfoProto roleInfoProto = null;
            if (r2CGetRoles.RoleInfoList.Count == 0)
            {
                // 创建角色
                C2R_CreateRole c2RCreateRole = C2R_CreateRole.Create();
                c2RCreateRole.Account = account;
                c2RCreateRole.Token = response.Token;
                c2RCreateRole.ServerId = serverInfoProto.Id;
                c2RCreateRole.RoleName = r2CGetRoles.RoleInfoList[0].Name;
                R2C_CreateRole r2CCreateRole = await clientSenderComponent.Call(c2RCreateRole) as R2C_CreateRole;
                if (r2CCreateRole.Error != ErrorCode.ERR_Success)
                {
                    Log.Error($"创建角色失败 Error:{r2CCreateRole.Error}");
                    return;
                }
                
                roleInfoProto = r2CCreateRole.RoleInfo;
            }
            else
            {
                // 选择角色
                roleInfoProto = r2CGetRoles.RoleInfoList[0];
            }
            
            // 获取RealmKey
            C2R_GetRealmKey c2RGetRealmKey = C2R_GetRealmKey.Create();
            c2RGetRealmKey.Account = account;
            c2RGetRealmKey.Token = response.Token;
            c2RGetRealmKey.ServerId = serverInfoProto.Id;
            R2C_GetRealmKey r2CGetRealmKey = await clientSenderComponent.Call(c2RGetRealmKey) as R2C_GetRealmKey;
            if (r2CGetRealmKey.Error != ErrorCode.ERR_Success)
            {
                Log.Error($"获取 RealmKey Error:{r2CGetRealmKey.Error}");
                return;
            }
            
            // enter game map
            NetClient2Main_LoginGame netClient2MainLogin =
                    await clientSenderComponent.LoginGameAsync(account, r2CGetRealmKey.Key, roleInfoProto.Id, r2CGetRealmKey.Address);

            if (netClient2MainLogin.Error != ErrorCode.ERR_Success)
            {
                Log.Error($"进入游戏失败 Error:{netClient2MainLogin.Error}");
                return;
            }
            
            Log.Debug($"角色进入游戏成功 account:{account} roleId:{roleInfoProto.Id} address:{r2CGetRealmKey.Address}");

            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
        }
    }
}