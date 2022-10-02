using api.Services;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Devices;

var token = new DefaultAzureCredential();

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((hostContext,config) =>
    {
        config.AddUserSecrets<Program>();

        var configRoot = config.Build();

        if (!Uri.TryCreate(configRoot.GetValue<string>("APPCONFIG_ENDPOINT"), UriKind.Absolute, out var endpoint))
        {
            throw new InvalidOperationException("App configuration URI is not valid.");
        }

        config.AddAzureAppConfiguration(options => options.Connect(endpoint, new DefaultAzureCredential()));
    })
    .ConfigureServices((hostContext, services) =>
    {
        var iotHubHostName = hostContext.Configuration.GetValue<string>("iotHubHostName");

        services.AddSingleton<IIotHubService, IotHubService>();
        services.AddScoped(_ => ServiceClient.Create(iotHubHostName, new DefaultAzureCredential()));
        services.AddScoped(_ => RegistryManager.Create(iotHubHostName, new DefaultAzureCredential()));

        //services.AddApplicationInsightsTelemetry();
    })
    .Build();

await host.RunAsync();
