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

        public IotHubService(ILogger<IotHubService> logger, ServiceClient serviceClient, RegistryManager registryManager)
        {
            this.logger = logger;
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
    }
}
