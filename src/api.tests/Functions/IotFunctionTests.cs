using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using api.Functions;
using api.Services;
using Microsoft.Azure.Devices.Shared;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Core;

namespace api.tests.Functions
{
    [TestFixture]
    public class IotFunctionTests
    {
        private ILogger<IotFunction> subLogger;
        private IIotHubService subIotHubService;

        [SetUp]
        public void SetUp()
        {
            subLogger = Substitute.For<ILogger<IotFunction>>();
            subIotHubService = Substitute.For<IIotHubService>();
        }

        private IotFunction CreateIotFunction()
        {
            return new IotFunction(subLogger, subIotHubService);
        }

        [Test]
        public async Task Run_If_There_Is_Devices_Returns_Correct_Status_Code()
        {
            // Arrange
            var iotFunction = CreateIotFunction();
            subIotHubService.GetTwinsAsync().ReturnsForAnyArgs(GenerateDevices());

            var req = new MockHttpRequestData("");

            // Act
            var result = await iotFunction.Run(req, null);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        //[Test]
        //public async Task Run_If_There_Is_Devices_Returns_Correct_Json()
        //{
        //    // Arrange
        //    var iotFunction = CreateIotFunction();
        //    subIotHubService.GetTwinsAsync().ReturnsForAnyArgs(GenerateDevices());
        //    var twin = GenerateDevices(1).First().ToJson();

        //    var req = new MockHttpRequestData("");

        //    // Act
        //    var result = await iotFunction.Run(req, null);

        //    // Assert
        //    Assert.AreEqual(twin, ReadBody(result.Body));
        //}

        private static List<Twin> GenerateDevices(int numberofdevices = 1)
        {
            return Enumerable
                .Range(0, numberofdevices)
                .Select(i => new Twin { DeviceId = $"device{i}" })
                .ToList();
        }

        private static string ReadBody(Stream stream)
        {
            string body = "";

            var test = new StreamReader(stream).ReadToEnd();    

            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, 256);

            return body;
        }
    }
}
