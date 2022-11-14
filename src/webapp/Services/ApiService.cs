using webapp.Contracts;
using Shared.Dtos;
using Newtonsoft.Json;
using System.Text;

namespace webapp.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient client;
        private readonly ILogger<ApiService> logger;

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            this.logger = logger;
            client = httpClient;
        }

        public string GetBaseAddress() => client.BaseAddress!.ToString();

        public async Task<DeviceDto[]> GetDevicesAsync()
        {
            var response  = await client.GetStringAsync("/api/devices");
            var devices = JsonConvert.DeserializeObject<DeviceDto[]>(response);
            return devices ?? Array.Empty<DeviceDto>();
        }

        public async Task<SensorDto[]> GetSensorsAsync()
        {
            var response = await client.GetStringAsync("/api/sensors");
            var devices = JsonConvert.DeserializeObject<SensorDto[]>(response);
            return devices ?? Array.Empty<SensorDto>();
        }

        public async Task<DeviceSensorDto[]> GetSensorDataAsync(SensorDataRequestDto dto)
        {
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/sensordata", content);
            response.EnsureSuccessStatusCode();

            var sensors = JsonConvert.DeserializeObject<DeviceSensorDto[]>(await response.Content.ReadAsStringAsync());
            return sensors ?? Array.Empty<DeviceSensorDto>();
        }

        public async Task<string> GetToken()
        {
            var token = await client.GetStringAsync("/api/token");
            return token;
        }
    }
}