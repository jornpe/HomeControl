using webapp;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Polly;
using System.Net;
using webapp.Pages;
using Polly.Contrib.WaitAndRetry;
using webapp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Components.Web;

// Reusable HTTP retry policy for retrying up to 10 times on request timeout and status codes from 500 and above
var httpPolicy = Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(x => x.StatusCode is HttpStatusCode.RequestTimeout or >= HttpStatusCode.InternalServerError)
        .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 3));

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

if (!Uri.TryCreate(builder.Configuration.GetValue<string>("API_ENDPOINT"), UriKind.Absolute, out var apiEndpoint))
{
    throw new InvalidOperationException($"App configurationURI is not valid. URI: ${apiEndpoint}");
}

builder.Services.AddHttpClient<IotService>(client => client.BaseAddress = apiEndpoint)
    .AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
    .ConfigureHandler(authorizedUrls: new[] { apiEndpoint.AbsoluteUri }));

builder.Services.AddMsalAuthentication(options =>
{
    options.ProviderOptions.DefaultAccessTokenScopes.Add(builder.Configuration.GetValue<string>("AzureApiScope"));
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
});

await builder.Build().RunAsync();
