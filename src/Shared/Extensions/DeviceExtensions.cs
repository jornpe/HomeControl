using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Shared.Dtos;

namespace Shared.Extensions
{
    public static class DeviceExtensions
    {
        public static DeviceDto ConvertToDto(this Twin twin)
        {
            return new DeviceDto { 
                Id = twin.DeviceId, 
                State = twin.ConnectionState ?? DeviceConnectionState.Disconnected, 
                LastActivityTime = twin.LastActivityTime 
            };
        }
    }
}
