using Devices.Shared;
using Devices.Shared.Enums;
using Iot.Device.Common;
using Iot.Device.DHTxx.Esp32;
using nanoFramework.Azure.Devices.Client;
using nanoFramework.Azure.Devices.Shared;
using nanoFramework.Json;
using nanoFramework.Networking;
using System;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnitsNet;

namespace Dht22Sensor
{
    public class Program
    {
        const string devideId = "HomeOfficeDevice";
        const string iotHubAddress = "iot-homeproject.azure-devices.net";
        const string sasKey = "QW0P1I55OLfb6ASdNDspLFWw4DlEC/df5opfZrsQH+0=";
        const string ssid = "Duni19";
        const string password = "Eddie2007";
        const string reportingPropertyName = "ReportingIntervalSeconds";
        const int dhtEchoPin = 26;
        const int dhtTriggerPin = 27;
        static int reportingInterval = 1800; // default is every 30 min
        static string temp;
        static string humidity;
        static string dewPoint;
        static string heatIndex;
        static long time;
        static bool isLastReadSuccessful;

        public static void Main()
        {
            bool wifiConnected = false;
            while(!wifiConnected)
            {
                Debug.WriteLine($"Trying to connect to SSID: {ssid}");
                CancellationTokenSource cts = new(60000); // waiting total of 1 min for wifi connection to occur. 

                wifiConnected = WifiNetworkHelper.ConnectDhcp(ssid, password, token: cts.Token);

                if (!wifiConnected)
                {
                    Debug.WriteLine($"Can't connect to the network, error: {WifiNetworkHelper.Status}");
                    if (WifiNetworkHelper.HelperException is not null)
                    {
                        Debug.WriteLine($"ex: {WifiNetworkHelper.HelperException}");
                    }
                    Thread.Sleep(30000); // Wait 30sec then try again. 
                }
            }

            Debug.WriteLine("Connected to wifi!!");

            X509Certificate azureCert = new (AzureRootCA.RootCA);

            DeviceClient device = new(iotHubAddress, devideId, sasKey, azureCert: azureCert);
            var isOpen = device.Open();
            Debug.WriteLine($"Connection is open: {isOpen}");

            while (true)
            {
                var twin = device.GetTwin(new CancellationTokenSource(5000).Token);

                Temperature dhtTemp;
                RelativeHumidity dhtHumidity;

                if (twin is not null && twin.Properties.Desired.Contains(reportingPropertyName))
                {
                    reportingInterval = (int)twin.Properties.Desired[reportingPropertyName];
                    Debug.WriteLine($"Updating reporting interval to {reportingInterval}");
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

                    Debug.WriteLine($"Temperature: {temp}\u00B0C, Relative humidity: {humidity}%, HeatIndex: {heatIndex}, DewPoint: {dewPoint}");

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

                    TwinCollection reported = new TwinCollection();
                    reported.Add("LastTempReading", temp);
                    reported.Add("LastHumidityReading", humidity);
                    reported.Add("LastDewPointReading", dewPoint);
                    reported.Add("LastHeatIndexReading", heatIndex);
                    reported.Add("LastSensorRadingTime", time);
                    reported.Add("Firmware", "nanoFramework");

                    var update = device.UpdateReportedProperties(reported, new CancellationTokenSource(5000).Token);

                    if (update) Debug.WriteLine("Successfully updated device twin");
                    else Debug.WriteLine("Could not update device twin");

                    var send = device.SendMessage(json, new CancellationTokenSource(10000).Token);

                    if (send) Debug.WriteLine("Successfully sent message to IOT hub");
                    else Debug.WriteLine("Could not send message to IOT hub");
                }
                else
                {
                    Debug.WriteLine("Error while reading temperature sensor data");
                }

                Thread.Sleep(reportingInterval * 1000);
            }
        }

        
    }
}
