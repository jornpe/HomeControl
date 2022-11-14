using Shared.Enums;

namespace Shared.Dtos
{
    public class SensorDataRequestDto
    {
        public string DeviceId { get; init; }
        public long RangeStart { get; init; }
        public long RangeEnd { get; init; }
        public SensorType[] Sensors { get; init; }
    }
}
