using Shared.Dtos;
using System.Net.Http.Json;
using System.Text.Json;
using webapp.Contracts;

namespace webapp.Services
{
    public class UnauthApiService : IUnauthApiService
    {
        private readonly HttpClient client;
        private readonly ILogger<ApiService> logger;

        public UnauthApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            this.logger = logger;
            client = httpClient;
        }

        public string GetBaseAddress() => client.BaseAddress!.ToString();


        public async Task<JsonDocument?> GetOfficeTemp()
        {
            string dto = string.Empty;
            try
            {
                dto = await client.GetStringAsync("/api/officetemp") ?? string.Empty;
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
