using api.Models;
using Shared.Dtos;
using Shared.Enums;

namespace api.Contracts
{
    public interface IDbClient
    {
        Task AddSensorDataToDb(SensorDb data, string deviceId);
        IAsyncEnumerable<SensorDb> GetSensorData(long rangeStart, long rangeEnd, string deviceId, SensorType[] sensorType);
    }
}