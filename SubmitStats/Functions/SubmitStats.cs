using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using stat_parser;
using Nest;
using Utils;
using Models;
using QuakeStats.Services;
using Newtonsoft.Json;
using QuakeStats.Models;

namespace SubmitStats
{
    public static class SubmitStats
    {
        [FunctionName("SubmitStats")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [Queue("outqueue"), StorageAccount("AzureWebJobsStorage")] ICollector<string> msg,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // pull out body text
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                
            // deserialize 
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            dynamic data = requestBody;

            // store message in data queue
            if (string.IsNullOrEmpty(data.ToString()) || string.IsNullOrWhiteSpace(data.ToString()) || data == null)
            {
                return new BadRequestObjectResult("Your match stats payload was empty. Please send correct payload.");
            }

            // Add a message to the output collection.
            msg.Add(data.ToString());

            CrmodStatParser parser = null; 
            try
            {
                parser = new CrmodStatParser(log);
                parser.SplitMatchStats(requestBody);
                parser.ProcessMatchInfo();
                parser.ProcessTeamStats();
                parser.ProcessPlayerStats();
                parser.ProcessQuadStats();
                parser.ProcessBadStats();
                parser.ProcessEfficiencyStats();
                parser.ProcessKillStats();
            }
            catch (Exception e)
            {
                await DiscordLogger.PostMessageToDiscordAsync("Error parsing CRMOD match results payload. Error: " + e.Message);
                return new BadRequestObjectResult("Unable to parse CRMOD match results payload. Error: " + e.Message);

            }


            var esClient = new ElasticService();

            // some horrible stuff going on here. I'm lazy to fix it but the purpose is to make the elastic->javascript parsing much easier... for my brain. 
            var matchToPost = mapToMatchModel(parser.MatchResults);


            // Post match data in entirety to elastic
            var asyncMatchIndexResponse = await esClient.PostMatch(matchToPost);

            if (asyncMatchIndexResponse.IsValid)
            {
                log.LogInformation($"Match data: '{parser.MatchResults.Id}' posted to elastic");
            }
            else
            {
                log.LogError($"Match data: '{parser.MatchResults.Id}' could NOT be posted to elastic");
            }

            //  upload all player stats
            foreach (var player in parser.MatchResults.ListOfPlayers)
            {
                if (int.TryParse(player.Key, out int statId))
                {
                    if (statId > 0)
                    {
                        var asyncPlayerIndexResponse = await esClient.PostPlayerStats(player.Value);
                        if (asyncPlayerIndexResponse.IsValid)
                            log.LogInformation($"Player: '{player.Key}' for match '{player.Value.MatchId}' posted to elastic");
                        else
                            log.LogError($"Player: '{player.Key}' for match '{player.Value.MatchId}' could NOT posted to elastic");
                    }
                }
                else
                    log.LogError($"Int32.TryParse could not parse '{player.Key}' to an int. In match '{player.Value.MatchId}'");
            }


            return data != null
                ? (ActionResult)new OkObjectResult("Match Stats Received.")
                : new BadRequestObjectResult("Your match stats payload was empty. Please send correct payload.");
        }

        private static MatchModel mapToMatchModel (MatchResultDTO dataToMap)
        {
            var match = new MatchModel();
            match.Id = dataToMap.Id;
            match.MatchDate = dataToMap.MatchDate;
            match.MapName = dataToMap.MapName;
            match.MatchTimeLimit = dataToMap.MatchTimeLimit;
            match.MatchType = dataToMap.MatchType;
            match.MatchMVP = dataToMap.MatchMVP;

            foreach (var team in dataToMap.ListOfTeams)
            {
                match.ListOfTeams.Add(team.Value);
                if (team.Value.TeamVerdict.Equals("win"))
                {
                    match.Winner = team.Value.TeamColor;
                    match.WinnerScore = team.Value.TeamTotalFrags;
                }
                else if (team.Value.TeamVerdict.Equals("lose"))
                {
                    match.LoserScore = team.Value.TeamTotalFrags;
                }
                else
                {
                    match.Winner = "tie";
                }
            }

            foreach (var player in dataToMap.ListOfPlayers)
            {
                match.ListOfPlayers.Add(player.Value);
            }

            return match;
        }
    }
}
