using System;
using System.Collections.Generic;

namespace Models
{
    public class MatchResultDTO
    {
        public Guid Id { get; set; }
        public string MatchType { get; set; }
        public string MapName { get; set; }
        public DateTime MatchDate { get; set; }
        public string MatchTimeLimit { get; set; }
        public Dictionary<string, MatchTeamStatDTO> ListOfTeams { get; set; }
        public Dictionary<string, MatchPlayerDTO> ListOfPlayers { get; set; }

        public MatchResultDTO()
        {
            Id = Guid.NewGuid();
            MatchDate = DateTime.Now;
            ListOfTeams = new Dictionary<string, MatchTeamStatDTO>();
            ListOfPlayers = new Dictionary<string, MatchPlayerDTO>();
        }

        public string MatchMVP { get; set; }
    }
}
