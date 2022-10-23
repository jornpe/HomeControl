using System.Net;
using System.Text;
using System.Text.Json;
using api.Contracts;
using api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Dtos;

namespace api.Functions
{
    public class IotFunction
    {
        private readonly ILogger logger;
        private readonly IIotHubService iotService;
        private readonly IIdentityService identityService;
        private readonly IConfiguration configuration;

        public IotFunction(ILoggerFactory loggerFactory, IIotHubService iotService, IIdentityService identityService, IConfiguration configuration)
        {
            logger = loggerFactory.CreateLogger<IotFunction>();
            this.iotService = iotService;
            this.identityService = identityService;
            this.configuration = configuration;
        }

        [Function("Devices")]
        public async Task<HttpResponseData> Devices([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            var isTokenValid = await identityService.ValidateAccess(req);

            if (!isTokenValid)
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            
            var devices = await iotService.GetTwinsAsync();
            var devicesDto = devices.Select(twin => (DeviceDto)twin);
            var json = JsonSerializer.Serialize(devicesDto);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("content-type", "application/json");
            await response.WriteStringAsync(json, Encoding.UTF8);
           
            return response;
        }

        [Function("Token")]
        public HttpResponseData Token([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
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

        [Function("OfficeTemp")]
        public async Task<HttpResponseData> OfficeTemp([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            var officeDevice = configuration["OfficeDevice"];
            string temp;
            string time;

            if (string.IsNullOrEmpty(officeDevice))
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            var properties = await iotService.GetReportedPropertiesForDeviceAsync(officeDevice);

            if (properties is null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            try
            {
                temp = properties["LastTempReading"];
                time = properties["LastSensorRadingTime"];
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error while getting temp property for device {officeDevice}");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            var dto = new
            {
                Temp = temp,
                Time = time
            };

            var json = JsonSerializer.Serialize(dto);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("content-type", "application/json");
            await response.WriteStringAsync(json, Encoding.UTF8);

            return response;
        }
    }
}
