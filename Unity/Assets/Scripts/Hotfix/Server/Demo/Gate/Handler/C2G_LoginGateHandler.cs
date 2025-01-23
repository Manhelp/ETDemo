using System;


namespace ET.Server
{
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_LoginGateHandler : MessageSessionHandler<C2G_LoginGate, G2C_LoginGate>
    {
        protected override async ETTask Run(Session session, C2G_LoginGate request, G2C_LoginGate response)
        {
            if (session.GetComponent<SessionLockingComponent>() != null)
            {
                response.Error = ErrorCode.ERR_RequestRepeatedly;
                return;
            }
            
            Scene root = session.Root();
            string account = root.GetComponent<GateSessionKeyComponent>().Get(request.Key);
            if (account == null)
            {
                response.Error = ErrorCore.ERR_ConnectGateKeyError;
                response.Message = "Gate key验证失败!";
                return;
            }
            
            root.GetComponent<GateSessionKeyComponent>().Remove(request.Key);
            session.RemoveComponent<SessionAcceptTimeoutComponent>();

            long instanceId = session.InstanceId;
            CoroutineLockComponent coroutineLockComponent = session.Root().GetComponent<CoroutineLockComponent>();
            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await coroutineLockComponent.Wait(CoroutineLockType.LoginGate, request.Account.GetLongHashCode()))
                {
                    // 不相同，可能session已发生变化
                    if (instanceId != session.InstanceId)
                    {
                        response.Error = ErrorCode.ERR_LoginGateError;
                        return;
                    }
                    
                    // 通知登录中心记录
                    G2L_AddLoginRecord g2LAddLoginRecord = G2L_AddLoginRecord.Create();
                    g2LAddLoginRecord.Account = account;
                    g2LAddLoginRecord.ServerId = root.Zone();
                    L2G_AddLoginRecord l2GAddLoginRecord = (L2G_AddLoginRecord)await root.GetComponent<MessageSender>()
                            .Call(StartSceneConfigCategory.Instance.LoginCenterConfig.ActorId, g2LAddLoginRecord);
                    if (l2GAddLoginRecord.Error != ErrorCode.ERR_Success)
                    {
                        response.Error = l2GAddLoginRecord.Error;
                        Log.Error($"登录中心注册失败 Error {l2GAddLoginRecord.Error}");
                        session.Disconnect().Coroutine();
                        return;
                    }
                    
                    PlayerComponent playerComponent = root.GetComponent<PlayerComponent>();
                    Player player = playerComponent.GetByAccount(account);
                    if (player == null)
                    {
                        player = playerComponent.AddChildWithId<Player, string>(request.RoleId, account);
                        player.UnitId = request.RoleId;
                
                        playerComponent.Add(player);
                        PlayerSessionComponent playerSessionComponent = player.AddComponent<PlayerSessionComponent>();
                        playerSessionComponent.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.GateSession);
                        await playerSessionComponent.AddLocation(LocationType.GateSession);
			
                        player.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
                        await player.AddLocation(LocationType.Player);
			
                        session.AddComponent<SessionPlayerComponent>().Player = player;
                        playerSessionComponent.Session = session;

                        player.State = PlayerState.Gate;
                    }
                    else
                    {
                        // 判断是否在战斗
                        // PlayerRoomComponent playerRoomComponent = player.GetComponent<PlayerRoomComponent>();
                        // if (playerRoomComponent.RoomActorId != default)
                        // {
                        //     CheckRoom(player, session).Coroutine();
                        // }
                        // else
                        {
                            player.RemoveComponent<PlayerOfflineOutTimeComponent>();
                    
                            session.AddComponent<SessionPlayerComponent>().Player = player;
                            player.GetComponent<PlayerSessionComponent>().Session = session;
                        }
                    }

                    response.UnitId = player.Id;
                }
            }
        }

        private static async ETTask CheckRoom(Player player, Session session)
        {
            Fiber fiber = player.Fiber();
            await fiber.WaitFrameFinish();

            G2Room_Reconnect g2RoomReconnect = G2Room_Reconnect.Create();
            g2RoomReconnect.PlayerId = player.Id;
            using Room2G_Reconnect room2GateReconnect = await fiber.Root.GetComponent<MessageSender>().Call(
                player.GetComponent<PlayerRoomComponent>().RoomActorId,
                g2RoomReconnect) as Room2G_Reconnect;
            G2C_Reconnect g2CReconnect = G2C_Reconnect.Create();
            g2CReconnect.StartTime = room2GateReconnect.StartTime;
            g2CReconnect.Frame = room2GateReconnect.Frame;
            g2CReconnect.UnitInfos.AddRange(room2GateReconnect.UnitInfos);
            session.Send(g2CReconnect);
            
            session.AddComponent<SessionPlayerComponent>().Player = player;
            player.GetComponent<PlayerSessionComponent>().Session = session;
        }
    }
}