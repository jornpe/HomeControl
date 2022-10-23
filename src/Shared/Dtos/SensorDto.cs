using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;

namespace Shared.Dtos
{
    public class SensorDto
    {
        public DeviceDto Device { get; set; }
        public SensorType sensorType { get; set; }
        public string Value { get; set; }
        public DateTime TimeStamp { get; set; }

    }
}
