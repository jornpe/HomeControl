using Microsoft.Azure.Devices;

namespace Shared.Dtos
{
    public class DeviceDto
    {
        public string Id { get; set; }
        public DeviceConnectionState State  { get; set; }
        public DateTime? LastActivityTime { get; set; }

    }   
}
