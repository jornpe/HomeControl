using System.Text.Json;
using api.Contracts;
using api.Models;
using api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Shared.Dtos;
using Shared.Enums;

namespace api.Functions
{
    public class IotHubFunction
    {
        private readonly ILogger logger;
        private readonly IDbClient dbClient;

        public IotHubFunction(ILoggerFactory loggerFactory, IDbClient client)
        {
            logger = loggerFactory.CreateLogger<IotHubFunction>();
            dbClient = client;
        }

        [Function("SaveSensorDataToDb")]
        public async Task Save([
            EventHubTrigger("%EventHubName%", Connection = "EventHubConnectionString")]
            string[] inputs)
        {
            if (inputs is null || inputs.Length == 0)
            {
                logger.LogError("Invalid input data received to function: SaveSensorDataToDb");
                return;
            }

            foreach (var input in inputs)
            {
                try
                {
                    var message = JsonSerializer.Deserialize<DeviceMessageDto>(input);

                    if (message is null)
                    {
                        logger.LogError("Message with no sensor data received");
                        return;
                    }

                    foreach (var item in message.Items)
                    {
                        switch (message.MessageType)
                        {
                            case MessageType.Sensor:
                                var sensor = JsonSerializer.Deserialize<DeviceSensorDto>((JsonElement)item);
                                if (sensor is null)
                                {
                                    logger.LogError("Could not parse incoming sensor data");
                                    continue;
                                }
                                var dbEntity = (SensorDb)sensor;
                                await dbClient.AddSensorDataToDb(dbEntity, message.Deviceid);
                                break;

                            case MessageType.Event:
                                throw new NotImplementedException("Event as message type is not implemented yet");
                            default:
                                throw new InvalidOperationException("Received unknown message type");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error while computing incoming message in function: SaveSensorDataToDb");
                }
                
            }
        }
    }
}