using Models;
using Nest;
using QuakeStats.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils;
using System.Linq;
using QuakeStats.Utils;

namespace QuakeStats.Services
{
    public class ElasticService
    {
        private ElasticClient client { get; set; }
        public ElasticService()
        {
            var elasticUri = ConfigHelper.GetEnvironmentVariable("elastic_url");
            var elasticUsername = ConfigHelper.GetEnvironmentVariable("elastic_name");
            var elasticValue = ConfigHelper.GetEnvironmentVariable("elastic_value");
            var settings = new ConnectionSettings(new Uri(elasticUri))
                .DefaultIndex("matches")
                .DefaultMappingFor<MatchPlayerDTO>(m => m.IndexName("players"))
                .DefaultMappingFor<MatchModel>(m => m.IndexName("match_stats"))
                .DefaultMappingFor<PlayerSignupModel>(m => m.IndexName("player_signup"))
                .BasicAuthentication(elasticUsername, elasticValue);

            client = new ElasticClient(settings);
        }


        public async Task<IndexResponse> PostMatch(MatchResultDTO match)
        {
            var asyncIndexResponse = await client.IndexDocumentAsync(match);
            if (asyncIndexResponse.IsValid)
            {
                await DiscordLogger.PostMessageToDiscordAsync($"New match posted: Match date: '{match.MatchDate}' with '{match.MatchMVP}' as the MVP!");
                return asyncIndexResponse;
            }
            else
            {
                await DiscordLogger.PostMessageToDiscordAsync($"Match data: '{match.Id}' could NOT posted to elastic");
                return asyncIndexResponse;
            }
        }

        public async Task<IndexResponse> PostMatch(MatchModel match)
        {
            var asyncIndexResponse = await client.IndexDocumentAsync(match);
            if (asyncIndexResponse.IsValid)
            {
                await DiscordLogger.PostMessageToDiscordAsync($"New match posted: Match date: '{match.MatchDate}' with '{match.MatchMVP}' as the MVP!");
                return asyncIndexResponse;
            }
            else
            {
                await DiscordLogger.PostMessageToDiscordAsync($"Match data: '{match.Id}' could NOT posted to elastic");
                return asyncIndexResponse;
            }
        }

        public async Task<IndexResponse> PostPlayer(PlayerSignupModel player)
        {
            var asyncIndexResponse = await client.IndexDocumentAsync(player);
            if (asyncIndexResponse.IsValid)
            {
                await DiscordLogger.PostMessageToDiscordAsync($"Signup data: Name: '{player.Name}' with Code: '{player.QuakeLoginCode}'  and ID: '{player.QuakeId}' posted to elastic");
                return asyncIndexResponse;
            }
            else
            {
                await DiscordLogger.PostMessageToDiscordAsync($"Signup data: Name: '{player.Name}' with Code: '{player.QuakeLoginCode}'  and ID: '{player.QuakeId}' NOT posted to elastic");
                return asyncIndexResponse;
            }
        }

        public async Task<IndexResponse> PostPlayerStats(MatchPlayerDTO player)
        {
            var asyncIndexResponse = await client.IndexDocumentAsync(player);
            if (asyncIndexResponse.IsValid)
            {
                return asyncIndexResponse;
            }
            else
            {
                return asyncIndexResponse;
            }
        }

        public async Task<IReadOnlyCollection<PlayerSignupModel>> GetSignedupPlayers()
        {
            var searchResults = await client.SearchAsync<PlayerSignupModel>(s => s
                .From(0)
                .Size(100)
                .MatchAll()
                .Scroll("5m")
            );

            return searchResults.Documents;
        }

        public async Task<List<string>> GetListOfQuakeIds()
        {
            var players = await GetSignedupPlayers();
            var ListOfIds = new List<string>();

            foreach (var player in players)
            {
                ListOfIds.Add(player.QuakeId);
            }

            return ListOfIds;
        }

        public async Task<Dictionary<string, string>> GetPlayerIdToNameMap()
        {
            var players = await GetSignedupPlayers();
            var IdToNames = new Dictionary<string, string>();

            foreach (var player in players)
            {
                IdToNames.Add(player.QuakeId, player.Name);
            }

            return IdToNames;
        }

        public async Task<IReadOnlyCollection<MatchModel>> GetMatches(string matchId)
        {
            IReadOnlyCollection<MatchModel> searchResults = null;
            if (!string.IsNullOrEmpty(matchId))
            {
                var temp = await client.SearchAsync<MatchModel>(s => s
                    .Query(q => q
                        .Match(m => m
                            .Field(f => f.Id)
                            .Query(matchId)
                        )
                    )
                );

                searchResults = temp.Documents;
            }
            else
            {
                var temp = await client.SearchAsync<MatchModel>(s => s
                    .From(0)
                    .Size(10)
                    .MatchAll()
                    .Sort(x => x.Descending(f => f.MatchDate))
                );

                searchResults = temp.Documents;
            }


            return searchResults;
        }

        internal async Task<List<AllStatsModel>> GetAllStats()
        {
            // fetch stats
            var response = await client.SearchAsync<MatchPlayerDTO>(q => q
/*                .Size(0)
                .Query(a => a
                    .MatchAll()
                    )*/
                .Aggregations(agg => agg
                .Terms(
                    "by_stat_id", e =>
                        e.Field("statId.keyword")
                            .Size(100)
                            .Aggregations(child => child
                                .Sum("total_quads", g => g.Script("Integer.parseInt(doc['numberOfQuads.keyword'].value)"))
                                .Sum("total_dmg", x => x.Script("Integer.parseInt(doc['dmgGiven.keyword'].value)"))
                                .Sum("total_q_enemy_kills", x => x.Script("Integer.parseInt(doc['numQuadEnemyKills.keyword'].value)"))
                                .Sum("total_q_self_kills", x => x.Script("Integer.parseInt(doc['numQuadSelfKills.keyword'].value)"))
                                .Sum("total_q_team_kills", x => x.Script("Integer.parseInt(doc['numQuadTeamKills.keyword'].value)"))
                                .Sum("total_frags", x => x.Script("Integer.parseInt(doc['numOfFrags.keyword'].value)"))
                                .Sum("total_enemy_kills", x => x.Script("Integer.parseInt(doc['numOfEnemyKills.keyword'].value)"))
                                .Sum("total_self_kills", x => x.Script("Integer.parseInt(doc['numOfSelfKills.keyword'].value)"))
                                .Sum("total_team_kills", x => x.Script("Integer.parseInt(doc['numOfTeamKills.keyword'].value)"))
                                .Sum("total_deaths", x => x.Script("Integer.parseInt(doc['numOfDeaths.keyword'].value)"))
                                .Sum("total_dmg_taken", x => x.Script("Integer.parseInt(doc['dmgTaken.keyword'].value)"))
                                .Sum("total_drop_packs", x => x.Script("Integer.parseInt(doc['droppedPaks.keyword'].value)"))

                            )
                    )
                )
            );

            // parse stats
            var results = response
                .Aggregations
                .Terms("by_stat_id")
                .Buckets
                .Select(e => new AllStatsModel()
                {
                    StatId = e.Key,
                    MatchCount = e.DocCount, /* total matches recorded */
                    avgDmgPerGame = (long)(e.SumBucket("total_dmg").Value / e.DocCount),
                    avgQuadsPerGame = (long)(e.SumBucket("total_quads").Value / e.DocCount),
                    avgQuadEnemyKillsPerGame = (long)(e.SumBucket("total_q_enemy_kills").Value / e.DocCount),
                    avgQuadSelfKillsPerGame = (long)(e.SumBucket("total_q_self_kills").Value / e.DocCount),
                    avgQuadTeamKillsPerGame = (long)(e.SumBucket("total_q_team_kills").Value / e.DocCount),
                    avgFragsPerGame = (long)(e.SumBucket("total_frags").Value / e.DocCount),
                    avgEnemyKillsPerGame = (long)(e.SumBucket("total_enemy_kills").Value / e.DocCount),
                    avgSelfKillsPerGame = (long)(e.SumBucket("total_self_kills").Value / e.DocCount),
                    avgTeamKillsPerGame = (long)(e.SumBucket("total_team_kills").Value / e.DocCount),
                    avgDeathsPerGame = (long)(e.SumBucket("total_deaths").Value / e.DocCount),
                    avgDmgTakenPerGame = (long)(e.SumBucket("total_dmg_taken").Value / e.DocCount),
                    avgDroppedPacksPerGame = (long)(e.SumBucket("total_drop_packs").Value / e.DocCount),
                }).ToList()
            ;

            return results;
        }
    }
}





