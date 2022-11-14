using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;

namespace Shared.Dtos
{
    public class DeviceDto
    {
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("connectionState")]
        public DeviceConnectionState State  { get; set; }

        [JsonProperty("lastActivityTime")]
        public DateTime? LastActivityTime { get; set; }

        [JsonProperty("properties")]
        public TwinCollection Properties { get; set; } =  new();

    }
}
