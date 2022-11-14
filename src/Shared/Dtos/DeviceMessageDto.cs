using Shared.Enums;
using System.Collections;

namespace Shared.Dtos
{
    public class DeviceMessageDto
    {
        public string Deviceid { get; set; }
        public MessageType MessageType { get; set; }
        public ArrayList Items { get; set; } = new ArrayList();

    }
}
