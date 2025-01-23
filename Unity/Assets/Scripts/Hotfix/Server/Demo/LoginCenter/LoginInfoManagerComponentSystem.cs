namespace ET.Server
{
    [EntitySystemOf(typeof(LoginInfoManagerComponent))]
    [FriendOf(typeof(LoginInfoManagerComponent))]
    public static partial class LoginInfoManagerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LoginInfoManagerComponent self)
        {
            
        }

        [EntitySystem]
        private static void Destroy(this LoginInfoManagerComponent self)
        {
            self.LoginInfoDic.Clear();
        }

        public static void Add(this LoginInfoManagerComponent self, long key, int value)
        {
            if (self.LoginInfoDic.ContainsKey(key))
            {
                self.LoginInfoDic[key] = value;
                return;
            }
            self.LoginInfoDic.Add(key, value);
        }

        public static void Remove(this LoginInfoManagerComponent self, long key)
        {
            if (self.LoginInfoDic.ContainsKey(key))
            {
                self.LoginInfoDic.Remove(key);
            }
        }

        public static int Get(this LoginInfoManagerComponent self, long key)
        {
            if (self.LoginInfoDic.ContainsKey(key))
            {
                return self.LoginInfoDic[key];
            }
            return -1;
        }

        public static bool IsExist(this LoginInfoManagerComponent self, long key)
        {
            return self.LoginInfoDic.ContainsKey(key);
        }
    }
    
}