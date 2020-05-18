using System;
using System.Collections.Generic;
using System.Text;

namespace QuakeStats.Models
{
    internal class AllStatsModel
    {
        public string Name { get; set; }
        public string StatId { get; set; }
        public long? MatchCount { get; set; }
        public long avgDmgPerGame { get; set; }
        public long avgDmgTakenPerGame { get; set; }
        public long avgFragsPerGame { get; set; }
        public long avgEnemyKillsPerGame { get; set; }
        public long avgSelfKillsPerGame { get; set; }
        public long avgTeamKillsPerGame { get; set; }
        public long avgDeathsPerGame { get; set; }
        public long avgQuadsPerGame { get; set; }
        public long avgQuadEnemyKillsPerGame { get; set; }
        public long avgQuadSelfKillsPerGame { get; set; }
        public long avgQuadTeamKillsPerGame { get; set; }
        public long avgDroppedPacksPerGame { get; set; }
        public AllStatsModel()
        {
        }
    }
}

