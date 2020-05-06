using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QuakeStats.Services;

namespace Players
{
    public static class Players
    {
        [FunctionName("Players")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Queue("outqueue"), StorageAccount("AzureWebJobsStorage")] ICollector<string> msg, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            var client = new ElasticService();
            var searchResults = await client.GetSignedupPlayers();
         
            // TODO: what if search returns nothing?


            return searchResults != null
                ? (ActionResult)new OkObjectResult(new { players = searchResults })
                : new BadRequestObjectResult("Players not found.");
        }
    }
}
