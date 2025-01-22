namespace ET
{
    public enum RoleInfoState
    {
        Normal = 0,
        Freeze = 1,
    }
    
    [ChildOf]
    public class RoleInfo : Entity, IAwake
    {
        public string RoleName;
        public string Account;
        public int ServerId;
        public int State;
        public long LastLoginTime;
        public long CreateTime;
    }

}