using System.Net.Http.Json;
using webapp.Contracts;
using Shared.Dtos;
using System.Text.Json;

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
            var devices  = await client.GetFromJsonAsync<DeviceDto[]>("/api/devices");            
            return devices ?? Array.Empty<DeviceDto>();
        }

        public async Task<string> GetToken()
        {
            var token = await client.GetStringAsync("/api/token");
            return token;
        }
    }
}