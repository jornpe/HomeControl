using webapp;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Azure.Functions.Authentication.WebAssembly;
using Polly;
using System.Net;
using webapp.Pages;
using Polly.Contrib.WaitAndRetry;
using webapp.Services;

// Reusable HTTP retry policy for retrying up to 10 times on request timeout and status codes from 500 and above
var httpPolicy = Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(x => x.StatusCode is HttpStatusCode.RequestTimeout or >= HttpStatusCode.InternalServerError)
        .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 3));

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddHttpClient<WeatherService>().AddPolicyHandler(httpPolicy);

builder.Services.AddStaticWebAppsAuthentication();

await builder.Build().RunAsync();
