using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using api.Functions;
using Microsoft.Azure.Devices.Shared;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Core;
using api.Contracts;

namespace api.tests.Functions
{
    [TestFixture]
    public class IotFunctionTests
    {
        private ILoggerFactory? subLoggerFactory;
        private IIotHubService? subIotHubService;
        private IIdentityService? identityService;

        [SetUp]
        public void SetUp()
        {
            subLoggerFactory = Substitute.For<ILoggerFactory>();
            subIotHubService = Substitute.For<IIotHubService>();
            identityService = Substitute.For<IIdentityService>();
        }

        private IotFunction CreateIotFunction()
        {
            return new IotFunction(subLoggerFactory!, subIotHubService!, identityService!);
        }

        [Test]
        public void Run_If_There_Is_Devices_Returns_Correct_Status_Code()
        {
            // Arrange
            var req = new MockHttpRequestData("");
            var iotFunction = CreateIotFunction();
            subIotHubService!.GetTwinsAsync().ReturnsForAnyArgs(GenerateDevices());
            identityService!.ValidateAccess(req).ReturnsForAnyArgs(true);

            // Act
            var result = iotFunction.Devices(req).Result;

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
