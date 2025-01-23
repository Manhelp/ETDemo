﻿namespace ET.Server
{
    public enum PlayerState
    {
        Disconnect,
        Gate,
        Game,
    }
    
    [ChildOf(typeof(PlayerComponent))]
    public sealed class Player : Entity, IAwake<string>
    {
        public string Account { get; set; }
        
        public PlayerState State { get; set; }
        
        public long UnitId { get; set; }
    }
}