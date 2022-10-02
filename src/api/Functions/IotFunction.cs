using System.Net;
using System.Text;
using System.Text.Json;
using api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace api.Functions
{
    public class IotFunction
    {
        private readonly ILogger logger;
        private readonly IIotHubService iotService;

        public IotFunction(ILogger<IotFunction> logger, IIotHubService iotService)
        {
            this.logger = logger;
            this.iotService = iotService;
        }

        [Function("IotFunction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "devices")] HttpRequestData req, 
            FunctionContext executionContext)
        {
            var devices = await iotService.GetTwinsAsync().ConfigureAwait(false);
            var json = JsonSerializer.Serialize(devices);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("content-type", "application/json");
            response.WriteString(json, Encoding.UTF8);
           
            return response;
        }
    }
}
