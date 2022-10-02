using System.Net;
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
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, FunctionContext executionContext)
        {
            var devices = await iotService.GetTwinsAsync();
            var json = JsonSerializer.Serialize(devices.Select(x => x.ToJson()));

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(json);
           
            return response;
        }
    }
}
