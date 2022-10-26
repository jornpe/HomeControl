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
            string dto = await client.GetStringAsync("/api/officetemp") ?? string.Empty;

            if (string.IsNullOrEmpty(dto))
            {
                return null;
            }

            return JsonDocument.Parse(dto);
        }
    }
}
