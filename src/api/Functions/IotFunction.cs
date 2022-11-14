using System.Net;
using System.Text;
using api.Contracts;
using api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Dtos;
using Shared.Enums;

namespace api.Functions
{
    public class IotFunction
    {
        private readonly ILogger logger;
        private readonly IIotHubService iotService;
        private readonly IIdentityService identityService;
        private readonly IConfiguration configuration;
        private readonly IDbClient dbClient;

        public IotFunction(ILoggerFactory loggerFactory, IIotHubService iotService, IIdentityService identityService, IConfiguration configuration, IDbClient dbClient)
        {
            logger = loggerFactory.CreateLogger<IotFunction>();
            this.iotService = iotService;
            this.identityService = identityService;
            this.configuration = configuration;
            this.dbClient = dbClient;
        }

        [Function("Devices")]
        public async Task<HttpResponseData> GetDevices([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            var isTokenValid = await identityService.ValidateAccess(req);

            if (!isTokenValid)
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            
            var devices = await iotService.GetTwinsAsync();
            var json = JsonConvert.SerializeObject(devices);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("content-type", "application/json");
            await response.WriteStringAsync(json, Encoding.UTF8);
           
            return response;
        }

        [Function("Sensors")]
        public async Task<HttpResponseData> GetSensors([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            try
            {
                var isTokenValid = await identityService.ValidateAccess(req);

                if (!isTokenValid)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                var devices = await iotService.GetTwinsAsync();
                List<SensorDto> dto = new();

                foreach (var device in devices)
                {
                    if (device.Properties.Reported.Contains("Sensors"))
                    {
                        var sensors = Convert.ToString(device.Properties.Reported["Sensors"]);

                        if (string.IsNullOrEmpty(sensors)) continue;

                        foreach (var sensor in sensors.Split(','))
                        {
                            if (Enum.TryParse(sensor, out SensorType sensorType))
                            {
                                long time = Convert.ToInt64(device.Properties.Reported["SensorRadingTime"]);
                                double value = Convert.ToDouble(device.Properties.Reported[sensor]);

                                dto.Add(new SensorDto
                                {
                                    DeviceId = device.DeviceId,
                                    SensorType = sensorType,
                                    EpocTime = time,
                                    Value = value
                                });
                            }
                        }
                    }
                }

                var json = JsonConvert.SerializeObject(dto);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("content-type", "application/json");
                await response.WriteStringAsync(json, Encoding.UTF8);

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception while getting sensors");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync(ex.Message, Encoding.UTF8);

                return response;
            }
        }

        [Function("SensorData")]
        public async Task<HttpResponseData> GetSensorData([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            try
            {
                var isTokenValid = await identityService.ValidateAccess(req);

                if (!isTokenValid)
                    return req.CreateResponse(HttpStatusCode.Unauthorized);

                List<DeviceSensorDto> sensorData = new();

                // read the contents of the posted data into a string
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonConvert.DeserializeObject<SensorDataRequestDto>(requestBody);

                if (request is null) return req.CreateResponse(HttpStatusCode.BadRequest);
                
                await foreach (var entity in dbClient.GetSensorData(request.RangeStart, request.RangeEnd, request.DeviceId, request.Sensors))
                {
                    sensorData.Add((DeviceSensorDto)entity);
                }

                var json = JsonConvert.SerializeObject(sensorData);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("content-type", "application/json");
                await response.WriteStringAsync(json, Encoding.UTF8);

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception while getting sensors");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync(ex.Message, Encoding.UTF8);

                return response;
            }
        }

        [Function("Token")]
        public HttpResponseData GetToken([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            string token;
            try
            {
                token = IdentityService.GetAccessToken(req);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception while sending back token");
                token = "Found no token in the request  header: " + ex.Message;
            } 
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("content-type", "application/text");
            response.WriteString(token, Encoding.UTF8);

            return response;
        }

        [Function("OfficeTemp")]
        public async Task<HttpResponseData> GetOfficeTemp([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            var officeDevice = configuration["OfficeDevice"];
            double temp = default;
            long time = default;

            if (string.IsNullOrEmpty(officeDevice))
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            try
            {
                var properties = await iotService.GetReportedPropertiesForDeviceAsync(officeDevice);

                if (properties is null)
                {
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                // Need to check all sensors and get the sensor type we need as the reported sensor type can be
                // stored as number or string of the enum sensor type
                var sensors = Convert.ToString(properties["Sensors"]);

                foreach (var sensor in sensors.Split(','))
                {
                    if (Enum.TryParse(sensor, out SensorType sensorType))
                    {
                        if (sensorType == SensorType.Temperature)
                        {
                            temp = properties[sensor];
                            time = properties["SensorRadingTime"];
                        }
                    }
                }
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

            var json = JsonConvert.SerializeObject(dto);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("content-type", "application/json");
            await response.WriteStringAsync(json, Encoding.UTF8);

            return response;
        }
    }
}
