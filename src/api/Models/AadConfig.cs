namespace api.Models
{
    public class AadConfig
    {
        public string Audience { get; set; }
        public string Tenant { get; set; }
        public string TenantId { get; set; }
        public string ValidIssuer => $"https://login.microsoftonline.com/{TenantId}/v2.0";
        public string AuthorityConfig => $"https://login.microsoftonline.com/{Tenant}/v2.0/.well-known/openid-configuration";
    }
}
