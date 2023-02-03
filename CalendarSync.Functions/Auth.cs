using CalendarSync.Functions.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CalendarSync.Functions
{
    public static class Auth
    {
        [FunctionName("Auth")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                var body = JsonSerializer.Deserialize<AuthRequest>(req.Body);

                var authService = new AuthService();
                var response = await authService.GetToken(body.RefreshToken);

                return new OkObjectResult(response);
            }
            catch (HttpRequestException e)
            {
                if (e.InnerException != null)
                {
                    log.LogError(e.InnerException, "Inner exception");
                }
                
                log.LogError(e, "Top-level error");
                return new BadRequestObjectResult(e);
            }
            catch (Exception e)
            {
                log.LogError(e, e.Message);
                return new BadRequestObjectResult(e);
            }
        }

        private static async Task<RefreshTokenResponse> GetToken(string refreshToken)
        {
            var values = new Dictionary<string, string>()
            {
                ["client_id"] = "",
                ["grant_type"] = "refresh_token",
                ["scope"] = "offline_access Calendars.ReadWrite",
                ["refresh_token"] = refreshToken,
            };
            var body = new FormUrlEncodedContent(values);

            using var client = new HttpClient();
            using var response = await client.PostAsync("https://login.microsoftonline.com/common/oauth2/v2.0/token", body);

            response.EnsureSuccessStatusCode();
            using var stream = await response.Content.ReadAsStreamAsync();
            var responseBody = await JsonSerializer.DeserializeAsync<RefreshTokenResponse>(stream);
            return responseBody;
        }
    }
}
