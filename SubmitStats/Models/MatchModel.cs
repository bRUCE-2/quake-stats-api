using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuakeStats.Models
{
    public class MatchModel
    {
        public Guid Id { get; set; }
        public string MatchType { get; set; }
        public string MapName { get; set; }
        public DateTime MatchDate { get; set; }
        public string Winner { get; set; }
        public string WinnerScore { get; set; }
        public string LoserScore { get; set; }
        public string MatchTimeLimit { get; set; }
        public List<MatchTeamStatDTO> ListOfTeams { get; set; }
        public List<MatchPlayerDTO> ListOfPlayers { get; set; }
        public string MatchMVP { get; set; }

        public MatchModel()
        {

            ListOfTeams = new List<MatchTeamStatDTO>();
            ListOfPlayers = new List<MatchPlayerDTO>();
        }
    }
}
