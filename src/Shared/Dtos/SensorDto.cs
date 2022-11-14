using Shared.Enums;

namespace Shared.Dtos
{
    public class SensorDto
    {
        public string DeviceId { get; set; }
        public SensorType SensorType { get; set; }
        public double Value { get; set; }
        public long EpocTime { get; set; }
        public string SensorUnit => SensorType switch
        {
            SensorType.Temperature => "°C",
            SensorType.Humidity => "%",
            SensorType.DewPoint => "°C",
            SensorType.HeatIndex => "°C",
            _ => throw new ArgumentOutOfRangeException(nameof(SensorType), $"Not expected value for: {SensorType}")
        };
    }
}
