using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QuakeStats.Services;

namespace QuakeStats.Functions
{
    public static class GetAllStats
    {
        [FunctionName("AllStats")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Queue("outqueue"), StorageAccount("AzureWebJobsStorage")] ICollector<string> msg, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            var client = new ElasticService();
            var searchResults = await client.GetAllStats();
            var names = await client.GetPlayerIdToNameMap();

            foreach(var playerStat in searchResults)
            {
                playerStat.Name = names[playerStat.StatId];
            }

            // TODO: what if search returns nothing?


            return searchResults != null
                ? (ActionResult)new OkObjectResult(new { allStats = searchResults })
                : new BadRequestObjectResult("Players stats not found.");
        }
    }
}
