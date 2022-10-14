using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;
using RichardSzalay.MockHttp;
using Shared.Dtos;
using System.Net;
using System.Net.Http.Json;
using webapp.Services;

namespace webapp.tests.Services
{
    [TestFixture]
    public class IotServiceTests
    {
        private ILogger<IotService> subLogger;
        private MockHttpMessageHandler mockHttp;

        [SetUp]
        public void SetUp()
        {
            subLogger = Substitute.For<ILogger<IotService>>();
            mockHttp = new MockHttpMessageHandler();
        }

        private IotService CreateService(HttpClient client)
        {
            client.BaseAddress = new Uri("http://localhost:7071");
            return new IotService(client, subLogger);
        }

        [Test]
        public async Task GetDevicesAsync_WhenReceivingOneDevice_ReturnOneDeviceDto()
        {
            // Arrange
            mockHttp.When("http://localhost:7071/api/devices")
                    .Respond("application/json", "[{\"Id\":\"sim-device\",\"State\":0,\"LastActivityTime\":\"0001-01-01T00:00:00Z\"}]");

            var client = mockHttp.ToHttpClient();
            var service = CreateService(client);

            // Act
            var result = await service.GetDevicesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Has.Exactly(1).Items);
            Assert.That(result.First().Id, Is.EqualTo("sim-device"));
        }

        [Test]
        public async Task GetDevicesAsync_WhenReceivingOZeroDevices_ReturnEmptyArray()
        {
            // Arrange
            mockHttp.When("http://localhost:7071/api/devices")
                    .Respond("application/json", "[]");

            var client = mockHttp.ToHttpClient();
            var service = CreateService(client);

            // Act
            var result = await service.GetDevicesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Exactly(0).Items);
        }

        [Test]
        public async Task GetDevicesAsync_WhenUnauthorized_ReturnEmptyArray()
        {
            // Arrange
            mockHttp.When("http://localhost:7071/api/devices")
                    .Respond(HttpStatusCode.Unauthorized);

            var client = mockHttp.ToHttpClient();
            var service = CreateService(client);

            // Act
            var result = await service.GetDevicesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Exactly(0).Items);
        }

        [Test]
        public async Task GetToken_WhenSendingToken_GetTokenAsPlainTextBack()
        {
            // Arrange
            var token = "SomeToken";
            mockHttp.When("http://localhost:7071/api/token")
                   .WithHeaders("Authorization", "Bearer SomeToken")
                   .Respond("text/plain", token);

            var client = mockHttp.ToHttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var service = CreateService(client);

            // Act
            var result = await service.GetToken();

            // Assert
            Assert.That(result, Is.EqualTo(token));
        }
    }
}
