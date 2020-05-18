using QuakeStats.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class DiscordLogger
    {
        public static async Task PostMessageToDiscordAsync(string message)
        {
            using (var httpClient = new HttpClient())
            {
                var discordWebhook = ConfigHelper.GetEnvironmentVariable("discord_webhook");
                var requestData = new StringContent("{\"content\":\"" + message + "\"}", Encoding.UTF8, "application/json");
                httpClient.BaseAddress = new Uri(discordWebhook);
                var result = await httpClient.PostAsync(httpClient.BaseAddress, requestData);
                string resultContent = await result.Content.ReadAsStringAsync();
            }
        }
    }
}
