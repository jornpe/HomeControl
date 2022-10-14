using System.Globalization;

namespace api.Services
{
    // demo code, usually want to pull these from key vault or config etc.
    internal static class Constants
    {
        internal static string audience = "https://funcapi.jornp.onmicrosoft.com"; // Get this value from the expose an api, audience uri section example https://appname.tenantname.onmicrosoft.com
        internal static string clientID = "32c129d0-4889-4c16-b7a4-707192e43fbe"; // this is the client id, also known as AppID. This is not the ObjectID
        internal static string tenant = "jornp.onmicrosoft.com"; // this is your tenant name
        internal static string tenantid = "96e79336-1e20-4d6e-b420-c665129d51d7"; // this is your tenant id (GUID)

        // rest of the values below can be left as is in most circumstances
        internal static string aadInstance = "https://login.microsoftonline.com/{0}/v2.0";
        internal static string authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
        internal static List<string> validIssuers = new List<string>()
            {
                $"https://login.microsoftonline.com/{tenant}/",
                $"https://login.microsoftonline.com/{tenant}/v2.0",
                $"https://login.windows.net/{tenant}/",
                $"https://login.microsoft.com/{tenant}/",
                $"https://sts.windows.net/{tenantid}/"
            };
    }
}