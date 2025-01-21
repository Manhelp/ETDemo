namespace ET.Server
{
    [EntitySystemOf(typeof(AccountSessionComponent))]
    [FriendOfAttribute(typeof(ET.Server.AccountSessionComponent))]
    public static partial class AccountSessionComponentSystem
    {
        [EntitySystem]
        private static void Awake(this AccountSessionComponent self)
        {

        }

        [EntitySystem]
        private static void Destroy(this AccountSessionComponent self)
        {
            self.AccountSessions.Clear();
        }

        public static Session Get(this AccountSessionComponent self, string account)
        {
            if (self.AccountSessions.TryGetValue(account, out EntityRef<Session> session))
            {
                return session;
            }

            return null;
        }

        public static void Add(this AccountSessionComponent self, string account, EntityRef<Session> session)
        {
            if (self.AccountSessions.ContainsKey(account))
            {
                self.AccountSessions[account] = session;
            }
            else
            {
                self.AccountSessions.Add(account, session);
            }
        }

        public static void Remove(this AccountSessionComponent self, string account)
        {
            if (self.AccountSessions.ContainsKey(account))
            {
                self.AccountSessions.Remove(account);
            }
        }
    }

}