using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using webapp;
using Microsoft.Azure.Functions.Authentication.WebAssembly;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

if (!Uri.TryCreate(builder.Configuration.GetValue<string>("API_ENDPOINT"), UriKind.Absolute, out var apiEndpoint))
{
    throw new InvalidOperationException($"App configurationURI is not valid. URI: ${apiEndpoint}");
}

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = apiEndpoint });

builder.Services.AddStaticWebAppsAuthentication();

await builder.Build().RunAsync();
