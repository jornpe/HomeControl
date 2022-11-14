using Azure;
using Azure.Data.Tables;
using Shared.Dtos;
using Shared.Enums;
using System.Text.Json.Serialization;

namespace api.Models
{
    public record SensorDb : ITableEntity
    {
        [JsonPropertyName("Time")]
        public string PartitionKey { get; set; } = default!; // Time stamp in epoch format for sensor reading

        [JsonPropertyName("Type")]
        public string RowKey { get; set; } = default!; // Sensor type

        [JsonIgnore]
        public ETag ETag { get; set; } = default!;

        [JsonIgnore]
        public DateTimeOffset? Timestamp { get; set; } = default!;

        [JsonPropertyName("Value")]
        public string SensorValue { get; set; } = default!;

        public static explicit operator SensorDb(DeviceSensorDto sensor)
        {
            return new SensorDb
            {
               PartitionKey = sensor.Time.ToString(),
               RowKey = sensor.Type.ToString(),
               SensorValue = sensor.Value.ToString("N1"),
            };
        }

        public static explicit operator DeviceSensorDto(SensorDb db)
        {
            return new DeviceSensorDto
            {
                Time = long.Parse(db.PartitionKey),
                Value = double.Parse(db.SensorValue),
                Type = (SensorType)Enum.Parse(typeof(SensorType), db.RowKey)
            };
        }
    }
}
