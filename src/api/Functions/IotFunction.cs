using System.Net;
using System.Text;
using System.Text.Json;
using api.Contracts;
using api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Shared.Extensions;

namespace api.Functions
{
    public class IotFunction
    {
        private readonly ILogger logger;
        private readonly IIotHubService iotService;
        private readonly IIdentityService identityService;

        public IotFunction(ILoggerFactory loggerFactory, IIotHubService iotService, IIdentityService identityService)
        {
            logger = loggerFactory.CreateLogger<IotFunction>();
            this.iotService = iotService;
            this.identityService = identityService;
        }

        [Function("Devices")]
        public async Task<HttpResponseData> Devices([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            var isTokenValid = await identityService.ValidateAccess(req);

            if (!isTokenValid)
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            

            var devices = await iotService.GetTwinsAsync();
            var devicesDto = devices.Select(d => d.ConvertToDto());
            var json = JsonSerializer.Serialize(devicesDto);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("content-type", "application/json");
            response.WriteString(json, Encoding.UTF8);
           
            return response;
        }

        [Function("token")]
        public HttpResponseData Token([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            string token;
            try
            {
                token = IdentityService.GetAccessToken(req);
            }
            catch (Exception ex)
            {
                token = "Found no token in the request  header: " + ex.Message;
            } 
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("content-type", "application/text");
            response.WriteString(token, Encoding.UTF8);

            return response;
        }
    }
}
