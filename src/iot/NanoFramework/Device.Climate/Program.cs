using Devices.Shared;
using Devices.Shared.Dtos;
using Iot.Device.Common;
using Iot.Device.DHTxx.Esp32;
using nanoFramework.Azure.Devices.Client;
using nanoFramework.Azure.Devices.Shared;
using nanoFramework.Json;
using nanoFramework.Networking;
using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnitsNet;

namespace Device.Climate
{
    public class Program
    {
        const string devideId = "<Your Device ID>";
        const string iotHubAddress = "<iot namespace address>.azure-devices.net";
        const string sasKey = "<SAS primary or secondary key>";
        const string ssid = "<WIFI SSID>";
        const string password = "<WIFI Password>";

        const string reportingPropertyName = "ReportingIntervalSeconds";
        const int dhtEchoPin = 26;
        const int dhtTriggerPin = 27;
        static int reportingInterval = 900; // default is every 15 min
        static string temp;
        static string humidity;
        static string dewPoint;
        static string heatIndex;
        static long time;
        static bool isLastReadSuccessful;

        public static void Main()
        {
            bool wifiConnected = false;
            while (!wifiConnected)
            {
                Console.WriteLine($"Trying to connect to SSID: {ssid}");
                CancellationTokenSource cts = new(60000); // waiting total of 1 min for wifi connection to occur. 

                //The parameter requiresDateTime is optional, and when set to true, will wait for the system to receive
                //a valid date and time from an SNTP service.This is paramount, for example, on devices connecting to
                //Azure IoT Hub that need to validate the server certificate and maybe generate keys which depend on accurate date and time.
                wifiConnected = WifiNetworkHelper.ConnectDhcp(ssid, password, requiresDateTime: true, token: cts.Token);

                if (!wifiConnected)
                {
                    Console.WriteLine($"Can't connect to the network, error: {WifiNetworkHelper.Status}");
                    if (WifiNetworkHelper.HelperException is not null)
                    {
                        Console.WriteLine($"ex: {WifiNetworkHelper.HelperException}");
                    }
                    Thread.Sleep(30000); // Wait 30sec then try again. 
                }
            }

            Console.WriteLine("Connected to wifi!!");

            X509Certificate azureCert = new(AzureRootCA.BaltimoreRootCA);

            DeviceClient device = new(iotHubAddress, devideId, sasKey, azureCert: azureCert);
            //DeviceClient device = new(iotHubAddress, devideId, sasKey);
            var isOpen = device.Open();
            Console.WriteLine($"Connection is open: {isOpen}");

            while (true)
            {
                var twin = device.GetTwin(new CancellationTokenSource(5000).Token);

                Temperature dhtTemp;
                RelativeHumidity dhtHumidity;

                if (twin is not null && twin.Properties.Desired.Contains(reportingPropertyName))
                {
                    reportingInterval = (int)twin.Properties.Desired[reportingPropertyName];
                    Console.WriteLine($"Updating reporting interval to {reportingInterval} seconds");
                }

                using (Dht11 dht = new(dhtEchoPin, dhtTriggerPin))
                {
                    // The temp sensor needs some time to get ready between readings. 
                    Thread.Sleep(2000);
                    dhtTemp = dht.Temperature;
                    Thread.Sleep(2000);
                    dhtHumidity = dht.Humidity;
                    Thread.Sleep(2000);
                    isLastReadSuccessful = dht.IsLastReadSuccessful;
                }

                if (isLastReadSuccessful)
                {

                    temp = dhtTemp.DegreesCelsius.ToString("N1");
                    humidity = dhtHumidity.Percent.ToString("N1");
                    heatIndex = WeatherHelper.CalculateHeatIndex(dhtTemp, dhtHumidity).DegreesCelsius.ToString("N1");
                    dewPoint = WeatherHelper.CalculateDewPoint(dhtTemp, dhtHumidity).DegreesCelsius.ToString("N1");
                    time = DateTime.UtcNow.ToUnixTimeSeconds();

                    Console.WriteLine($"Temperature: {temp}\u00B0C, Relative humidity: {humidity}%, HeatIndex: {heatIndex}, DewPoint: {dewPoint}");

                    var payload = new DeviceMessageDto
                    {
                        Deviceid = devideId,
                        MessageType = MessageType.Sensor,
                        Items = new ArrayList
                    {
                        new DeviceSensorDto
                        {
                            Type = SensorType.Tempsensor,
                            Time = time,
                            Value = temp
                        },
                        new DeviceSensorDto
                        {
                            Type = SensorType.HumiditySensor,
                            Time = time,
                            Value = humidity
                        },
                        new DeviceSensorDto
                        {
                            Type = SensorType.DewPoint,
                            Time = time,
                            Value = dewPoint
                        },
                        new DeviceSensorDto
                        {
                            Type = SensorType.HeatIndex,
                            Time = time,
                            Value = heatIndex
                        }
                    }
                    };

                    var json = JsonSerializer.SerializeObject(payload);

                    TwinCollection reported = new()
                {
                    { "LastTempReading", temp },
                    { "LastHumidityReading", humidity },
                    { "LastDewPointReading", dewPoint },
                    { "LastHeatIndexReading", heatIndex },
                    { "LastSensorRadingTime", time },
                    { "Firmware", "nanoFramework" }
                };

                    var update = device.UpdateReportedProperties(reported, new CancellationTokenSource(5000).Token);

                    if (update) Console.WriteLine("Successfully updated device twin");
                    else Console.WriteLine("Could not update device twin");

                    var send = device.SendMessage(json, new CancellationTokenSource(10000).Token);

                    if (send) Console.WriteLine("Successfully sent message to IOT hub");
                    else Console.WriteLine("Could not send message to IOT hub");
                }
                else
                {
                    Console.WriteLine("Error while reading temperature sensor data");
                }

                Thread.Sleep(reportingInterval * 1000);
            }
        }
    }
}
