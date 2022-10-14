
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;



namespace AzureADTokenValidation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IkN0VHVoTUptRDVNN0RMZHpEMnYyeDNRS1NSWSJ9.eyJhdWQiOiI3YjFjZTFhZC1hZjE1LTRlNWYtOWFlNC1hYWYwYTY4YTdhYjQiLCJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vZThlNmQwMTgtYTgzNC00MDZiLTlmNDMtMmU5NGFlNDI1ODc2L3YyLjAiLCJpYXQiOjE1ODkyODQ2OTEsIm5iZiI6MTU4OTI4NDY5MSwiZXhwIjoxNTg5Mjg4NTkxLCJhaW8iOiJBVVFBdS84UEFBQUEyNWpRNzJBc3IyWHBYMEJlUkZRNU1lTTdSLy8zbnpIbUxDUHNYekJYRWZpSGlkQWM4Y0RPNHJoUUVEdk56OWtnRTdPK1pYbmxNTTVRNmk4RjZYY0hLZz09IiwibmFtZSI6IlZpamFpIEFuYW5kIFJhbWFsaW5nYW0iLCJub25jZSI6IjY1OWM5MjU0LTQyN2YtNDg5MC05ODQ5LTU0ZTk1Yjc0NDYyNCIsIm9pZCI6ImU2YmFkYTg2LTk4NDktNGFhNC1hZWQ0LTg5YzZlZmE5YTc0MSIsInByZWZlcnJlZF91c2VybmFtZSI6InZpamFpYW5hbmRAQzk4Ni5vbm1pY3Jvc29mdC5jb20iLCJzdWIiOiJIdjhtQ3RDVkx1NW8wYklrSDJVd2RCTnVUWTlqeC1RNUU4LTVuYU5pdkFJIiwidGlkIjoiZThlNmQwMTgtYTgzNC00MDZiLTlmNDMtMmU5NGFlNDI1ODc2IiwidXRpIjoiVml0alZEcVh5RS0yaWNLQUlRT19BQSIsInZlciI6IjIuMCJ9.UAT3FkgCBYqM7Mfux1V-yF1QTqg0Dlz4Y2G8VQqNqg3WXWdQWf8v4MHcrZVzycV6FSA0-C4ANRpkBxeX1mdmtic4l6e5onOsRS3r_PsWpp7mew_XlTt9TQ1W1pO5dn6lw6J4U3k41kmXVAPwH9hbZNEmVVM6KjNQLW-SdCfaJJIB0XVIqEK2HOlBPxSI8hugh9S5yRMYz6-xi7SrG-wQJtsa9s7Wz5O4FYW2YmjHdUIdj_xwJbfS6_rknJetO756okz4tHY70N3GAKlr_zvfXvuAMjXfsXQNQN5-TQnDcWVkvK6SrhCGQunlPmjvvTvJyp7KLZVrRhxnz8w98yaEfA";
            string myTenant = "e8e6d018-a834-406b-9f43-2e94ae425876";
            var myAudience = "7b1ce1ad-af15-4e5f-9ae4-aaf0a68a7ab4";
            var myIssuer = String.Format(CultureInfo.InvariantCulture, "https://login.microsoftonline.com/{0}/v2.0", myTenant);
            var mySecret = "t.GDqjoBYBhB.tRC@lbq1GdslFjk8=57";
            var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));
            var stsDiscoveryEndpoint = String.Format(CultureInfo.InvariantCulture, "https://login.microsoftonline.com/{0}/.well-known/openid-configuration", myTenant);
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever());
            var config = await configManager.GetConfigurationAsync();

            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidAudience = myAudience,
                ValidIssuer = myIssuer,
                IssuerSigningKeys = config.SigningKeys,
                ValidateLifetime = false,
                IssuerSigningKey = mySecurityKey
            };

            var validatedToken = (SecurityToken)new JwtSecurityToken();

            // Throws an Exception as the token is invalid (expired, invalid-formatted, etc.)  
            tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            Console.WriteLine(validatedToken);
            Console.ReadLine();
        }
    }
}
