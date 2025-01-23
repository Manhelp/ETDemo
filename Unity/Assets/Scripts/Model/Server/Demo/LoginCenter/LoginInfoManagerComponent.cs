using System.Collections.Generic;

namespace ET.Server
{

    [ComponentOf(typeof(Scene))]
    public class LoginInfoManagerComponent : Entity, IAwake, IDestroy
    {
        public Dictionary<long, int> LoginInfoDic = new Dictionary<long, int>();
    }

}