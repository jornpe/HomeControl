using api.Models;

namespace api.Contracts
{
    public interface IDbClient
    {
        Task AddSensorDataToDb(SensorDb data, string deviceId);
    }
}