using Devices.Shared;
using Devices.Shared.Dtos;
using Iot.Device.Common;
using Iot.Device.DHTxx.Esp32;
using nanoFramework.Azure.Devices.Client;
using nanoFramework.Azure.Devices.Shared;
using nanoFramework.Hardware.Esp32;
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
        const string devideId = "HomeOfficeDevice";
        const string iotHubAddress = "iot-homeproject.azure-devices.net";
        const string sasKey = "zaFNQ2MJA3a+/PsoKKhtWd4bHOC85jo035V8ynHcTmk=";
        const string ssid = "Duni19";
        const string password = "Eddie2007";

        const string reportingPropertyName = "ReportingIntervalSeconds";
        const int dhtEchoPin = 26;
        const int dhtTriggerPin = 27;
        static int reportingInterval = 900; // default is every 15 min
        static double temp;
        static double humidity;
        static double dewPoint;
        static double heatIndex;
        static long time;
        static bool isLastReadSuccessful;

        public static void Main()
        {
            // Wrap everything inside a try clause so it always are able to go to sleep even if it crashes. 
            try
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

                    temp = dhtTemp.DegreesCelsius;
                    humidity = dhtHumidity.Percent;
                    heatIndex = WeatherHelper.CalculateHeatIndex(dhtTemp, dhtHumidity).DegreesCelsius;
                    dewPoint = WeatherHelper.CalculateDewPoint(dhtTemp, dhtHumidity).DegreesCelsius;
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
                                Type = SensorType.Temperature,
                                Time = time,
                                Value = temp
                            },
                            new DeviceSensorDto
                            {
                                Type = SensorType.Humidity,
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
                        { "Sensors", $"{SensorType.Temperature},{SensorType.Humidity},{SensorType.DewPoint},{SensorType.HeatIndex}" },
                        { SensorType.Temperature.ToString(), temp },
                        { SensorType.Humidity.ToString(), humidity },
                        { SensorType.DewPoint.ToString(), dewPoint },
                        { SensorType.HeatIndex.ToString(), heatIndex },
                        { "SensorRadingTime", time },
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

                GoToSleep();
            }
            catch (Exception)
            {
                GoToSleep();
            }
            

            static void GoToSleep()
            {
                Console.WriteLine($"Going to sleep for {reportingInterval} seconds");
                Sleep.EnableWakeupByTimer(TimeSpan.FromSeconds(reportingInterval));
                Sleep.StartDeepSleep();
            }
        }
    }
}
