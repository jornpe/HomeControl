using Microsoft.Azure.Devices.Shared;

namespace api.Contracts
{
    public interface IIotHubService
    {
        public Task<List<Twin>> GetTwinsAsync();
        public Task<TwinCollection?> GetReportedPropertiesForDeviceAsync(string deviceId);
    }
}
