using Microsoft.Azure.Functions.Worker.Http;

namespace api.Contracts
{
    public interface IIdentityService
    {
        Task<bool> ValidateAccess(HttpRequestData req);
    }
}