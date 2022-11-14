using Shared.Dtos;

namespace webapp.Contracts
{
    public interface IApiService
    {
        public string GetBaseAddress();
        public Task<DeviceDto[]> GetDevicesAsync();
        public Task<SensorDto[]> GetSensorsAsync();
        public Task<DeviceSensorDto[]> GetSensorDataAsync(SensorDataRequestDto dto);
        public Task<string> GetToken();
    }
}