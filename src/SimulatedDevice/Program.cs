using System.Collections;
using System.Dynamic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Shared.Dtos;
using Shared.Enums;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.Development.json")
    .Build();


var deviceConnectionString = config.GetConnectionString("IotDevice");
var device = DeviceClient.CreateFromConnectionString(deviceConnectionString);

var deviceName = deviceConnectionString
                        .Split(';')
                        .First(s => s.StartsWith("DeviceId"))
                        .Split('=')
                        .Last();

Console.WriteLine("Connected using device ID: " + deviceName);

var rec = ReceiveMessagesAsync(device);
var snd = SendTelemetryDataAsync(device, deviceName);

await Task.WhenAll(rec, snd);

static async Task ReceiveMessagesAsync(DeviceClient? device)
{
    if (device == null) return;

    Console.WriteLine("Starting to listen to incoming messages");

    while (true)
    {
        var receivedMessage = await device.ReceiveAsync();
        if (receivedMessage == null) continue;

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Received message: {Encoding.ASCII.GetString(receivedMessage.GetBytes())}");
        Console.ResetColor();

        await device.CompleteAsync(receivedMessage);
    }
}


static async Task SendTelemetryDataAsync(DeviceClient? device, string deviceName)
{
    if (device == null) return;

    Console.WriteLine("Starting sending telemetry messages");

    var rand = new Random();

    while (true)
    {
        //var temp = rand.Next(-20, 20);
        //var humidity = rand.Next(0, 100);

        //var reportedProperties = new TwinCollection
        //{
        //    ["CurrentTemp"] = temp,
        //    ["CurrentHumidity"] = humidity
        //};

        //await device.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);

        Console.WriteLine("Press anykey to send a new message");
        Console.ReadKey();

        var payload = new DeviceMessageDto
        {
            Deviceid = "sim-device",
            MessageType = MessageType.Sensor,
            Items = new ArrayList
            {
                new DeviceSensorDto
                {
                    Type = SensorType.Tempsensor,
                    Time = DateTime.UtcNow.Ticks,
                    Value = "20.0"
                }
            }
        };

    //string json = $$""" 
    //              {
    //                "deviceid": "sim-device",
    //                "messagetype": "sensorReading",
    //                "sensors": [
    //                  {
    //                    "type": "temperature",
    //                    "value": "10.2",
    //                    "time": {{DateTime.UtcNow.Ticks}}
    //                  },
    //                  {
    //                    "type": "humidity",
    //                    "value": "56",
    //                    "time": {{DateTime.UtcNow.Ticks}}
    //                  }
    //                ]
    //              }
    //              """;

        var json = JsonSerializer.Serialize(payload);
        var msg = new Message(Encoding.ASCII.GetBytes(json));
        msg.ContentType = "application/json";
        msg.ContentEncoding = "UTF-8";

        await device.SendEventAsync(msg);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Sending telemetry data at {DateTime.Now} with message : {json}");
        Console.ResetColor();

    }

}


