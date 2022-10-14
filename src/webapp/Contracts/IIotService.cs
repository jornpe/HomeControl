using Shared.Dtos;

namespace webapp.Contracts
{
    public interface IIotService
    {
        string GetBaseAddress();
        Task<DeviceDto[]> GetDevicesAsync();
        Task<string> GetToken();
    }
}