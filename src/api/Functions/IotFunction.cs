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

        public IotFunction(ILoggerFactory loggerFactory, IIotHubService iotService)
        {
            logger = loggerFactory.CreateLogger<IotFunction>();
            this.iotService = iotService;
        }

        [Function("Devices")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            var devices = iotService.GetTwinsAsync();
            var json = JsonSerializer.Serialize(devices);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("content-type", "application/json");
            response.WriteString(json, Encoding.UTF8);
           
            return response;
        }
    }
}
