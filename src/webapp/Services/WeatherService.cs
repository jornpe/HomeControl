using Polly.Contrib.WaitAndRetry;
using Polly;
using System.Net;
using Common;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace webapp.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            if (!Uri.TryCreate(configuration.GetValue<string>("API_ENDPOINT"), UriKind.Absolute, out var apiEndpoint))
            {
                throw new InvalidOperationException($"App configurationURI is not valid. URI: ${apiEndpoint}");
            }

            _httpClient = httpClient;
            _httpClient.BaseAddress = apiEndpoint;
            
        }

        public string GetBaseAddress() => _httpClient.BaseAddress.ToString();

        public async Task<WeatherForecastModel[]?> GetWeatherForecastAsync() =>
            await _httpClient.GetFromJsonAsync<WeatherForecastModel[]>("/api/WeatherForecast");
    }
}
