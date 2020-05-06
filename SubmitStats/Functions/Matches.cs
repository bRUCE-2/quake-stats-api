using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QuakeStats.Services;
using Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using QuakeStats.Models;

namespace QuakeStats.Functions
{
    public static class Matches
    {
        [FunctionName("Matches")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Queue("outqueue"), StorageAccount("AzureWebJobsStorage")] ICollector<string> msg, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var client = new ElasticService();

            // if match id == null, fetch last 10 matches
            string matchId = req.Query["id"];

            IReadOnlyCollection<MatchModel> searchResults = null;

            if (string.IsNullOrEmpty(matchId) || string.IsNullOrWhiteSpace(matchId))
            {
                searchResults = await client.GetMatches("");
            }
            else
            {
                searchResults = await client.GetMatches(matchId);
            }

            return searchResults != null
                ? (ActionResult)new OkObjectResult(new { matches = searchResults })
                : new BadRequestObjectResult("Matches not found.");
        }
    }
}
