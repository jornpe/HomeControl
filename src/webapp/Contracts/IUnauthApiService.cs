using System.Text.Json;

namespace webapp.Contracts
{
    public interface IUnauthApiService
    {
        string GetBaseAddress();
        Task<JsonDocument?> GetOfficeTemp();
    }
}