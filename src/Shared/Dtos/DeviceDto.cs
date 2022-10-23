using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using System.Runtime.CompilerServices;

namespace Shared.Dtos
{
    public class DeviceDto
    {
        public string Id { get; set; }
        public DeviceConnectionState State  { get; set; }
        public DateTime? LastActivityTime { get; set; }
        public bool IsOfficeDevice { get; set; } = false; // This will be used to show the current temp in the home office. That call will not require authorization

        public static explicit operator DeviceDto(Twin twin)
        {
            return new DeviceDto
            {
                Id = twin.DeviceId,
                State = twin.ConnectionState ?? DeviceConnectionState.Disconnected,
                LastActivityTime = twin.LastActivityTime
            };
        }

    }
}
