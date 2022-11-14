using Shared.Enums;

namespace Shared.Dtos
{
    public class DeviceSensorDto
    {
        public SensorType Type { get; set; }
        public double Value { get; set; }
        public long Time { get; set; }
    }
}
