namespace ET.Server
{
    [Invoke(TimerInvokeType.AccountCheckoutTime)]
    public class AccountSessionCheckoutTimer : ATimer<AccountCheckoutTimeComponent>
    {
        protected override void Run(AccountCheckoutTimeComponent t)
        {
            t?.DeleteSession();
        }
    }

    [EntitySystemOf(typeof(AccountCheckoutTimeComponent))]
    [FriendOfAttribute(typeof(ET.Server.AccountCheckoutTimeComponent))]
    public static partial class AccountCheckoutTimeComponentSystem
    {
        [EntitySystem]
        private static void Awake(this AccountCheckoutTimeComponent self, string account)
        {
            self.Account = account;
            self.Root().GetComponent<TimerComponent>().Remove(ref self.Timer);
            self.Timer = self.Root().GetComponent<TimerComponent>()
                    .NewOnceTimer(TimeInfo.Instance.ServerNow() + 600000, TimerInvokeType.AccountCheckoutTime, self);
        }

        [EntitySystem]
        private static void Destroy(this AccountCheckoutTimeComponent self)
        {
            self.Root().GetComponent<TimerComponent>().Remove(ref self.Timer);
        }

        public static void DeleteSession(this AccountCheckoutTimeComponent self)
        {
            Session session = self.GetParent<Session>();

            Session originalSession = session.Root().GetComponent<AccountSessionComponent>().Get(self.Account);
            if (originalSession != null && session.InstanceId == originalSession.InstanceId)
            {
                session.Root().GetComponent<AccountSessionComponent>().Remove(self.Account);
            }

            A2C_Disconnect acct = A2C_Disconnect.Create();
            acct.Error = 1;
            session?.Send(acct);
            session?.Disconnect().Coroutine();
        }
    }

}