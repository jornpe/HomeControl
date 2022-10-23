using Shared.Dtos;
using System.Text.Json;

namespace webapp.Contracts
{
    public interface IApiService
    {
        public string GetBaseAddress();
        public Task<DeviceDto[]> GetDevicesAsync();
        public Task<string> GetToken();
        public Task<JsonDocument?> GetOfficeTemp();
    }
}