namespace ET.Server
{
    [EntitySystemOf(typeof(TokenComponent))]
    [FriendOf(typeof(TokenComponent))]
    public static partial class TokenComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TokenComponent self)
        {
            
        }

        public static void Add(this TokenComponent self, string key, string token)
        {
            self.Tokens.Add(key, token);
            self.TimeoutRemoveKey(key, token).Coroutine();
        }

        public static string Get(this TokenComponent self, string key)
        {
            self.Tokens.TryGetValue(key, out var token);
            return token;
        }

        public static void Remove(this TokenComponent self, string key)
        {
            if (self.Tokens.ContainsKey(key))
            {
                self.Tokens.Remove(key);
            }
        }

        private static async ETTask TimeoutRemoveKey(this TokenComponent self, string key, string token)
        {
            await self.Root().GetComponent<TimerComponent>().WaitAsync(600 * 1000);

            string onlineToken = self.Get(key);

            if (!string.IsNullOrEmpty(onlineToken) && onlineToken == token)
            {
                self.Remove(key);
            }
        }
    }

}