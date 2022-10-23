using api.Contracts;
using api.Models;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using System.Text.RegularExpressions;

namespace api.Services
{
    public class DbClient : IDbClient
    {
        private readonly TableServiceClient client;
        private readonly ILogger<DbClient> logger;

        public DbClient(ILoggerFactory loggerFactory, TableServiceClient client)
        {
            logger = loggerFactory.CreateLogger<DbClient>();
            this.client = client;
        }

        public async Task AddSensorDataToDb(SensorDb data, string deviceId)
        {
            var tableName = GetTableName(MessageType.Sensor, deviceId);
            var tableClient = client.GetTableClient(tableName);

            await tableClient.CreateIfNotExistsAsync();
            await tableClient.AddEntityAsync(data);

            logger.LogTrace("Successfully added sensor data to table {tableName}", tableName);
        }

        public string GetTableName(MessageType messageType, string deviceId)
        {
            var name = $"{deviceId}{messageType}";
            return Regex.Replace(name, @"[^0-9a-zA-Z]+", "");
        }

    }
}
