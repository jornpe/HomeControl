using api.Contracts;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;

namespace api.Services
{
    public class IotHubService : IIotHubService
    {
        private readonly ILogger<IotHubService> logger;
        private readonly ServiceClient serviceClient;
        private readonly RegistryManager registryManager;

        public IotHubService(ILoggerFactory loggerFactory, ServiceClient serviceClient, RegistryManager registryManager)
        {
            logger = loggerFactory.CreateLogger<IotHubService>();
            this.serviceClient = serviceClient;
            this.registryManager = registryManager;
        }

        public async Task<List<Twin>> GetTwinsAsync()
        {
            var twins = new List<Twin>();
            var query = registryManager.CreateQuery("SELECT * from devices");

            while (query.HasMoreResults)
            {
                foreach (var twin in await query.GetNextAsTwinAsync())
                {
                    logger.LogInformation("Found device twin with ID: {twin.DeviceId}", twin.DeviceId);
                    twins.Add(twin);
                }
            }

            return twins;
        }

        public async Task<TwinCollection?> GetReportedPropertiesForDeviceAsync(string deviceId)
        {
            var twin = await registryManager.GetTwinAsync(deviceId);
            return twin?.Properties?.Reported;
        }
    }
}
