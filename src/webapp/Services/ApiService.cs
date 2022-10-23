using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using webapp.Contracts;
using Shared.Dtos;
using System.Text.Json.Serialization;
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
            var devices = Array.Empty<DeviceDto>();

            try
            {
                devices  = await client.GetFromJsonAsync<DeviceDto[]>("/api/devices");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Exception thrown while getting devices");
            }

            return devices ?? Array.Empty<DeviceDto>();
        }

        public async Task<string> GetToken()
        {
            string token = string.Empty;

            try
            {
                token = await client.GetStringAsync("/api/token") ?? string.Empty;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Exception thrown while getting token");
            }

            return token;
        }

        public async Task<JsonDocument?> GetOfficeTemp()
        {
            string dto = string.Empty;
            try
            {
                dto = await client.GetStringAsync("/api/unauthorized/officetemp") ?? string.Empty;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Exception thrown while getting token");
            }

            if (string.IsNullOrEmpty(dto))
            {
                return null;
            }

            return JsonDocument.Parse(dto);
        }
    }
}