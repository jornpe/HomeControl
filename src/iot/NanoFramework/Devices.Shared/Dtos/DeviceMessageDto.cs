using System.Collections;

namespace Devices.Shared.Dtos
{
    public class DeviceMessageDto
    {
        public string Deviceid { get; set; }
        public MessageType MessageType { get; set; }
        public ArrayList Items { get; set; } = new ArrayList();

    }
    public class DeviceSensorDto
    {
        public SensorType Type { get; set; }
        public string Value { get; set; }
        public long Time { get; set; }
    }

    public class DeviceEventDto
    {
        public string Event { get; set; }
        public long Time { get; set; }
    }
}
