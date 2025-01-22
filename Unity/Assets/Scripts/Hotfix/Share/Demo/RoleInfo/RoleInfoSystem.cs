namespace ET
{
    [EntitySystemOf(typeof(RoleInfo))]
    [FriendOf(typeof(RoleInfo))]
    public static partial class RoleInfoSystem
    {
        [EntitySystem]
        private static void Awake(this RoleInfo self)
        {
            
        }

        public static void FromMessage(this RoleInfo self, RoleInfoProto roleInfoProto)
        {
            self.Account = roleInfoProto.Account;
            self.RoleName = roleInfoProto.Name;
            self.ServerId = roleInfoProto.ServerId;
            self.State = roleInfoProto.state;
            self.CreateTime = roleInfoProto.CreateTime;
            self.LastLoginTime = roleInfoProto.LastLoginTime;
        }

        public static RoleInfoProto ToMessage(this RoleInfo self)
        {
            RoleInfoProto roleInfoProto = RoleInfoProto.Create();
            roleInfoProto.Id = self.Id;
            roleInfoProto.Account = self.Account;
            roleInfoProto.ServerId = self.ServerId;
            roleInfoProto.Name = self.RoleName;
            roleInfoProto.state = self.State;
            roleInfoProto.CreateTime = self.CreateTime;
            roleInfoProto.LastLoginTime = self.LastLoginTime;
            
            return roleInfoProto;
        }
    }

}