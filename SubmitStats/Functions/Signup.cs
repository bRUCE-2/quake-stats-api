using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nest;
using Models;
using QuakeStats.Services;

namespace Signup
{
    public static class Signup
    {
        [FunctionName("Signup")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [Queue("outqueue"), StorageAccount("AzureWebJobsStorage")] ICollector<string> msg, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // pull out body text
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody) || string.IsNullOrWhiteSpace(requestBody))
            {
                return new BadRequestObjectResult("Your player signup payload was empty. Please send correct payload.");
            }

            // store message in data queue
            // Add a message to the output collection.
            msg.Add(requestBody.ToString());

            // deserialize 
            PlayerSignupModel data = JsonConvert.DeserializeObject<PlayerSignupModel>(requestBody);

            //TODO: SHOULD PROBABLY CHECK IF DATA IS GOOD
            if (string.IsNullOrEmpty(data.Name) || string.IsNullOrWhiteSpace(data.Name))
            {
                return new BadRequestObjectResult("Player Name was empty. Please submit payload with player name.");
            }

            if (string.IsNullOrEmpty(data.QuakeId) || string.IsNullOrWhiteSpace(data.QuakeId))
            {
                return new BadRequestObjectResult("Player QuakeId was empty. Please submit payload with player QuakeId.");
            }


            // Post player signup data to elastic
            var esService = new ElasticService();

            var ids = await esService.GetListOfQuakeIds();
            if (ids.Contains(data.QuakeId))
                return new BadRequestObjectResult($"QuakeId: '{data.QuakeId}' already exists. Please submit request again with new QuakeId.");


            var asyncIndexResponse = await esService.PostPlayer(data);
            if (asyncIndexResponse.IsValid)
                log.LogInformation($"Signup data: Name: '{data.Name}' with Code: '{data.QuakeLoginCode}'  and ID: '{data.QuakeId}' posted to elastic");
            else
                log.LogError($"Signup data: Name: '{data.Name}' with Code: '{data.QuakeLoginCode}'  and ID: '{data.QuakeId}' posted to elastic");

            return data != null
                ? (ActionResult)new OkObjectResult("Player is signed up.")
                : new BadRequestObjectResult("Unable to sign up player.");
        }
    }
}
