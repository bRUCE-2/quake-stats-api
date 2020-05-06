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
                var requestData = new StringContent("{\"content\":\"" + message + "\"}", Encoding.UTF8, "application/json");
                httpClient.BaseAddress = new Uri("https://discordapp.com/api/webhooks/705271568133259335/7B9yYYMKBbvVC-93CFVnFrd6IUsGkgypg2-1OQF0YKO0V6OEdWKfsg-7McE8c41ozAfI");
                var result = await httpClient.PostAsync(httpClient.BaseAddress, requestData);
                string resultContent = await result.Content.ReadAsStringAsync();
            }
        }
    }
}
