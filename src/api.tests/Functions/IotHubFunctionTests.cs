using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using api.Contracts;
using api.Functions;
using Google.Protobuf.WellKnownTypes;

namespace api.tests.Functions
{
    [TestFixture]
    public class IotHubFunctionTests
    {
        private ILoggerFactory? subLoggerFactory;
        private IDbClient? subDbClient;

        [SetUp]
        public void SetUp()
        {
            subLoggerFactory = Substitute.For<ILoggerFactory>();
            subDbClient = Substitute.For<IDbClient>();
        }

        private IotHubFunction CreateIotHubFunction()
        {
            return new IotHubFunction(subLoggerFactory!, subDbClient!);
        }

        [Test]
        public void Save_RecievesPAyload_DoesNotThrowExeption()
        {
            // Arrange
            var iotHubFunction = CreateIotHubFunction();
            string[] inputs = GetPayload();

            // Assert
            Assert.DoesNotThrowAsync(() => iotHubFunction.Save(inputs));
        }

        private static string[] GetPayload(int numberOfItems = 1)
        {
            var payload = new string[numberOfItems];

            for (int i = 0; i < numberOfItems; i++)
            {
                payload[i] = $$"""
                    {
                      "Items": [
                        {
                          "Time": {{DateTime.Now.Ticks}},
                          "Type": 0,
                          "Value": "{{numberOfItems}}"
                        },
                        {
                          "Time": {{DateTime.Now.Ticks}},
                          "Type": 1,
                          "Value": "{{numberOfItems}}"
                        },
                        {
                          "Time": {{DateTime.Now.Ticks}},
                          "Type": 2,
                          "Value": "{{numberOfItems}}"
                        },
                        {
                          "Time": {{DateTime.Now.Ticks}},
                          "Type": 3,
                          "Value": "{{numberOfItems}}"
                        }
                      ],
                      "Deviceid": "HomeOfficeDevice",
                      "MessageType": 0
                    }
                    """;
            }
            return payload;
        }
    }
}
