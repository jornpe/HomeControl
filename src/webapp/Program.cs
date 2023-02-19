using webapp;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Polly;
using System.Net;
using Polly.Contrib.WaitAndRetry;
using webapp.Services;
using Microsoft.AspNetCore.Components.Web;
using webapp.Contracts;
using Radzen;
using webapp.Model;

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

builder.Services.AddHttpClient<IApiService, ApiService>(client => client.BaseAddress = apiEndpoint)
    .AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
    .ConfigureHandler(authorizedUrls: new[] { apiEndpoint.AbsoluteUri }));

builder.Services.AddHttpClient<IUnauthApiService, UnauthApiService>(client => client.BaseAddress = apiEndpoint);

builder.Services.AddMsalAuthentication(options =>
{
    options.ProviderOptions.DefaultAccessTokenScopes.Add(builder.Configuration.GetValue<string>("AzureApiScope"));
    options.ProviderOptions.Cache.CacheLocation = "localStorage";
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
});

builder.Services.AddScoped<DialogService>();
builder.Services.AddSingleton<StateContainer>();

await builder.Build().RunAsync();

