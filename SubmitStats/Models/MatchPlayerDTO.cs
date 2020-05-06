

using System;

namespace Models
{
    public class MatchPlayerDTO
    {
        public Guid Id { get; set; }
        // info
        public Guid MatchId { get; set; }
        public bool MVP { get; set; }
        public string StatId { get; set; }
        public string IpAddress { get; set; }
        public string Ping { get; set; }
        public string Name { get; set; }
        // begin skill stats
        public string TeamColor { get; set; }
        public string KillEfficiency { get; set; }
        public string WeaponEfficiency { get; set; }
        // begin quad stats
        public string NumberOfQuads { get; set; }
        public string QuadEfficiency { get; set; }
        public string NumQuadEnemyKills { get; set; }
        public string NumQuadSelfKills { get; set; }
        public string NumQuadTeamKills { get; set; }
        // begin kill stats
        public string NumOfFrags { get; set; }
        public string NumOfEnemyKills { get; set; }
        public string NumOfSelfKills { get; set; }
        public string NumOfTeamKills { get; set; }
        public string NumOfDeaths { get; set; }
        public string DmgGiven { get; set; }
        public string DmgTaken { get; set; }
        // begin efficiency stats
        public string BulletEfficiency { get; set; }
        public string NailsEfficiency { get; set; }
        public string RocketEfficiency { get; set; }
        public string LightningEfficiency { get; set; }
        public string TotalEfficiency { get; set; }
        // begin bad stats
        public string DroppedPaks { get; set; }
        public string SelfDamage { get; set; }
        public string TeamDamage { get; set; }
    }
}
