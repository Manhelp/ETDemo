using System.Collections.Generic;

namespace ET.Server
{
    [EntitySystemOf(typeof(ServerInfoManagerComponent))]
    [FriendOf(typeof(ServerInfoManagerComponent))]
    [FriendOf(typeof(ServerInfo))]
    public static partial class ServerInfoManagerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ServerInfoManagerComponent self)
        {
            self.Load();
        }

        [EntitySystem]
        private static void Destroy(this ServerInfoManagerComponent self)
        {
            foreach (var serverInfoRef in self.ServerInfos)
            {
                ServerInfo serverInfo = serverInfoRef;
                serverInfo?.Dispose();
            }
            self.ServerInfos.Clear();
        }

        public static void Load(this ServerInfoManagerComponent self)
        {
            Destroy(self);

            Dictionary<int, StartZoneConfig> serverInfoConfigs = StartZoneConfigCategory.Instance.GetAll();

            foreach (var info in serverInfoConfigs.Values)
            {
                if(info.ZoneType != 1)
                    continue;
                
                ServerInfo serverInfo = self.AddChildWithId<ServerInfo>(info.Id);
                serverInfo.ServerName = info.DBName;
                serverInfo.Status = (int)ServerStatus.Normal;
                
                self.ServerInfos.Add(serverInfo);
            }
        }
    }

}