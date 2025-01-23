using System.Net;
using ET.Server;

namespace ET.Client
{
    [MessageHandler(SceneType.NetClient)]
    public class Main2NetClient_LoginGameHandler : MessageHandler<Scene, Main2NetClient_LoginGame, NetClient2Main_LoginGame>
    {
        protected override async ETTask Run(Scene root, Main2NetClient_LoginGame request, NetClient2Main_LoginGame response)
        {
            string account = request.Account;
            
            // 创建一个 gate session，并保存到 SessionComponent 中
            NetComponent netComponent = root.GetComponent<NetComponent>();
            Session gateSession = await netComponent.CreateRouterSession(NetworkHelper.ToIPEndPoint(request.Address), account, account);
            gateSession.AddComponent<ClientSessionErrorComponent>();
            root.GetComponent<SessionComponent>().Session = gateSession;
            
            C2G_LoginGate c2GLoginGameGate = C2G_LoginGate.Create();
            c2GLoginGameGate.Account = account;
            c2GLoginGameGate.Key = request.RealmKey;
            c2GLoginGameGate.RoleId = request.RoleId;
            G2C_LoginGate g2CLoginGameGate =  (G2C_LoginGate) await gateSession.Call(c2GLoginGameGate);
            if (g2CLoginGameGate.Error != ErrorCode.ERR_Success)
            {
                response.Error = g2CLoginGameGate.Error;
                Log.Error($"登录gate失败 ErrorCode: {g2CLoginGameGate.Error}");
                return;
            }
            
            Log.Debug($"登录Gate成功");
            
            C2G_EnterGame c2GEnterGame = C2G_EnterGame.Create();
            G2C_EnterGame g2CEnterGame =  (G2C_EnterGame) await gateSession.Call(c2GEnterGame);
            if (g2CEnterGame.Error != ErrorCode.ERR_Success)
            {
                response.Error = g2CEnterGame.Error;
                Log.Error($"进入游戏地图失败 error: {g2CEnterGame.Error}");
                return;
            }
            
            Log.Debug($"进入游戏地图成功");

            response.UnitId = g2CEnterGame.UnitId;
        }
    }

}