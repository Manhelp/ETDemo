namespace ET
{
    [EntitySystemOf(typeof(ServerInfo))]
    [FriendOf(typeof(ServerInfo))]
    public static partial class ServerInfoSystem
    {
        [EntitySystem]
        private static void Awake(this ServerInfo self)
        {
            
        }

        public static void FromMessage(this ServerInfo self, ServerInfoProto message)
        {
            self.ServerName = message.ServerName;
            self.Status = message.Status;
        }

        public static ServerInfoProto ToMessage(this ServerInfo self)
        {
            ServerInfoProto message = ServerInfoProto.Create();
            message.ServerName = self.ServerName;
            message.Status = self.Status;
            message.Id = (int)self.Id;
            
            return message;
        }
    }

}