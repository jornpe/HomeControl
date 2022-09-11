using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Security.Claims;

namespace webappApi
{
    public static class WeatherForecast
    {
        [FunctionName("WeatherForecast")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context,ClaimsPrincipal principal)
        {

            if (principal == null && !principal.Identity.IsAuthenticated)
            {
                log.LogWarning("Request to {0} was not authenticated", nameof(WeatherForecast));
                return new UnauthorizedResult();
            }

            log.LogInformation("Sending weather message...");

            string path = Path.Combine(context.FunctionAppDirectory, "sample-data", "weather.json");

            string responseMessage = await File.ReadAllTextAsync(path);

            return new OkObjectResult(responseMessage);
        }
    }
}
