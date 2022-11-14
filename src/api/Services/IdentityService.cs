using api.Contracts;
using api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using HttpRequestData = Microsoft.Azure.Functions.Worker.Http.HttpRequestData;

namespace api.Services
{
    public sealed class IdentityService : IIdentityService
    {
        private readonly AadConfig aadConfig;
        private readonly ILogger<IdentityService> logger;

        public IdentityService(ILoggerFactory loggerFactory, IOptions<AadConfig> aadOptions)
        {
            logger = loggerFactory.CreateLogger<IdentityService>();
            aadConfig = aadOptions.Value;
        }

        public static string GetAccessToken(HttpRequestData req)
        {
            var token = req.Headers?.First(h => h.Key == "Authorization").Value.FirstOrDefault();
            if (!string.IsNullOrEmpty(token) && token.Contains("Bearer"))
                return token.Replace("Bearer ", "");
            return string.Empty;
        }

        public async Task<bool> ValidateAccess(HttpRequestData req)
        {
            try
            {
                var token = GetAccessToken(req);

                if (string.IsNullOrEmpty(token))
                    return false;                

                var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                        aadConfig.AuthorityConfig,
                        new OpenIdConnectConfigurationRetriever());

                var config = await configManager.GetConfigurationAsync();
                ISecurityTokenValidator tokenValidator = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidAudience = aadConfig.Audience,
                    ValidIssuer = aadConfig.ValidIssuer,
                    IssuerSigningKeys = config.SigningKeys
                };

                var claimsPrincipal = tokenValidator.ValidateToken(token, validationParameters, out var validatedToken);

                // TODO: Validate claims to see if request has correct roles. 
                // https://learn.microsoft.com/en-us/azure/active-directory/develop/scenario-protected-web-api-verification-scope-app-roles?tabs=aspnetcore
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Error while validating token");
                return false;
            }

            return true;
        }
    }
}
