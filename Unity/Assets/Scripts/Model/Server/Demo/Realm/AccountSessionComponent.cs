using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class AccountSessionComponent : Entity, IAwake, IDestroy
    {
        public Dictionary<string, EntityRef<Session>> AccountSessions = new Dictionary<string, EntityRef<Session>>();
    }

}