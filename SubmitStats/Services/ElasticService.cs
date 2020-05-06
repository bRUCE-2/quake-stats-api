using Models;
using Nest;
using QuakeStats.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils;

namespace QuakeStats.Services
{
    public class ElasticService
    {
        private ElasticClient client { get; set; }
        public ElasticService()
        {
            var settings = new ConnectionSettings(new Uri("https://61f4e1ea557b4150afda1d3b7ffb846c.eastus2.azure.elastic-cloud.com:9243"))
                .DefaultIndex("matches")
                .DefaultMappingFor<MatchPlayerDTO>(m => m.IndexName("players"))
                .DefaultMappingFor<MatchModel>(m => m.IndexName("match_stats"))
                .DefaultMappingFor<PlayerSignupModel>(m => m.IndexName("player_signup"))
                .BasicAuthentication("", "");

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
    }
}





