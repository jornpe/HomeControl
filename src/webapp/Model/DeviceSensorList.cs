using Shared.Enums;

namespace webapp.Model
{
    public class DeviceSensorList
    {
        public string DeviceId { get; set; }
        public SensorType SensorType { get; set; }
        public string GetListItem => $"{DeviceId} : {SensorType}";
    }
}
