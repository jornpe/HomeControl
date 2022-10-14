using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace api.Contracts
{
    public interface IIdentityService
    {
        Task<bool> ValidateAccess(HttpRequestData req);
    }
}