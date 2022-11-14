using Azure.Data.Tables;
using NSubstitute;
using NUnit.Framework;
using api.Services;
using Microsoft.Extensions.Logging;
using Shared.Enums;

namespace api.tests.Services
{
    [TestFixture]
    public class DbClientTests
    {
        private TableServiceClient? subTableServiceClient;
        private ILoggerFactory? subLoggerFactory;

        [SetUp]
        public void SetUp()
        {
            subLoggerFactory = Substitute.For<ILoggerFactory>();
            subTableServiceClient = Substitute.For<TableServiceClient>();
        }

        private DbClient CreateDbClient()
        {
            return new DbClient(subLoggerFactory, subTableServiceClient);
        }

        [Test]
        [TestCase("simdevice", MessageType.Sensor, "simdeviceSensor")]
        [TestCase("sim-device", MessageType.Sensor, "simdeviceSensor")]
        public void GetSensorTableName_WhenOnlyCorrectCharacters_GiveCorrectTableName(string deviceid, MessageType messageType, string expected)
        {
            // Arrange
            var dbClient = CreateDbClient();

            // Act
            var result = dbClient.GetTableName(messageType, deviceid);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

    }
}
