using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace Canonical.Functions.UnitTests
{
    public class GenerateStateItemHashUnitTests
    {
        private static readonly ILogger logger = Substitute.For<ILogger>();
        private static readonly GenerateStateItemHash generateStateItemHash = new();

        [Fact]
        public async Task GenerateStateItemHash_WithIncorrectEventType_ShouldReturnBadRequestAsync()
        {
            var payload = new
            {
                eventType = "BORGAR",
                eventBody = new
                {
                    Id = "1234567891011121314",
                    ShortName = "BORG1234",
                    FullName = "BORGAR1234"
                }
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var httpRequestMessage = GenerateHttpPostRequestMessage(jsonPayload);
            var response = await generateStateItemHash.Run(httpRequestMessage, logger) as ObjectResult;
            Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Event type BORGAR is not supported", response.Value.ToString());
        }

        [Fact]
        public async Task GenerateStateItemHash_WithoutProvidingEventBody_ShouldReturnBadRequestAsync()
        {
            var payload = new
            {
                eventType = "Youforce.OrganizationalUnit.Single"
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var httpRequestMessage = GenerateHttpPostRequestMessage(jsonPayload);
            var response = await generateStateItemHash.Run(httpRequestMessage, logger) as ObjectResult;
            Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("One or more required properties are not provided.", response.Value.ToString());
        }

        [Fact]
        public async Task GenerateStateItemHash_WithMalformedJson_ShouldReturnBadRequestAsync()
        {
            var payload = "\"eventType\": \"Youforce.OrganizationalUnit.Single\",\"eventBody\": {,";
            var httpRequestMessage = GenerateHttpPostRequestMessage(payload);
            var response = await generateStateItemHash.Run(httpRequestMessage, logger) as ObjectResult;
            Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Additional text encountered after finished reading JSON content", response.Value.ToString());
        }

        [Fact]
        public async Task GenerateStateItemHash_WithDifferentValuesOfUnusedFields_ShouldReturnSameHashSumsAsync()
        {
            var payload = new
            {
                eventType = "Youforce.OrganizationalUnit.Single",
                eventBody = new
                {
                    Id = "1234567890101112131",
                    ShortName = "FAO1234",
                    FullName = "FAKEORG1234",
                    Active = true,
                    Version = new
                    {
                        ValidityPeriod = new
                        {
                            VersionValidFrom = "2019-07-01",
                            VersionValidTo = "9999-12-31"
                        },
                        Managers = new[]
                        {
                            new {
                                PersonId = "1234567890123456789",
                                PersonNumber = "4321",
                                Name = "SomeRandomManager",
                                IsPrimary = true
                            }
                        }
                    }
                }
            };

            var payloadChangedId = new
            {
                eventType = "Youforce.OrganizationalUnit.Single",
                eventBody = new
                {
                    Id = "9876543212345678910",
                    ShortName = "FAO1234",
                    FullName = "FAKEORG1234",
                    Active = true,
                    Version = new
                    {
                        ValidityPeriod = new
                        {
                            VersionValidFrom = "2019-07-01",
                            VersionValidTo = "9999-12-31"
                        },
                        Managers = new[]
                        {
                            new {
                                PersonId = "1234567890123456789",
                                PersonNumber = "4321",
                                Name = "SomeRandomManager",
                                IsPrimary = true
                            }
                        }
                    }
                }
            };
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var httpRequestMessage = GenerateHttpPostRequestMessage(jsonPayload);
            var jsonPayloadChangedId = JsonConvert.SerializeObject(payloadChangedId);
            var httpRequestMessageChangedId = GenerateHttpPostRequestMessage(jsonPayloadChangedId);
            var response = await generateStateItemHash.Run(httpRequestMessage, logger) as ObjectResult;
            var responseChangedId = await generateStateItemHash.Run(httpRequestMessageChangedId, logger) as ObjectResult;
            Assert.Equal(response.Value.ToString(), responseChangedId.Value.ToString());
        }

        [Fact]
        public async Task GenerateStateItemHash_WithDifferentValuesOfMandatoryFields_ShouldReturnDifferenHashSumsAsync()
        {
            var payload = new
            {
                eventType = "Youforce.OrganizationalUnit.Single",
                eventBody = new
                {
                    Id = "1234567890101112131",
                    ShortName = "FAO1234",
                    FullName = "FAKEORG1234",
                    Active = true,
                    Version = new
                    {
                        ValidityPeriod = new
                        {
                            VersionValidFrom = "2019-07-01",
                            VersionValidTo = "9999-12-31"
                        },
                        Managers = new
                        {
                            PersonNumber = "4321"
                        }
                    }
                }
            };

            var payloadChangedOrgName = new
            {
                eventType = "Youforce.OrganizationalUnit.Single",
                eventBody = new
                {
                    Id = "1234567890101112131",
                    ShortName = "REO1234",
                    FullName = "REALORG1234",
                    Active = true,
                    Version = new
                    {
                        ValidityPeriod = new
                        {
                            VersionValidFrom = "2019-07-01",
                            VersionValidTo = "9999-12-31"
                        },
                        Managers = new
                        {
                            PersonNumber = "4321"
                        }
                    }
                }
            };
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var httpRequestMessage = GenerateHttpPostRequestMessage(jsonPayload);
            var jsonPayloadChangedOrgName = JsonConvert.SerializeObject(payloadChangedOrgName);
            var httpRequestMessageChangedOrgName = GenerateHttpPostRequestMessage(jsonPayloadChangedOrgName);
            var response = await generateStateItemHash.Run(httpRequestMessage, logger) as ObjectResult;
            var responseChangedOrgName = await generateStateItemHash.Run(httpRequestMessageChangedOrgName, logger) as ObjectResult;
            Assert.NotEqual(response.Value.ToString(), responseChangedOrgName.Value.ToString());
        }

        [Fact]
        public async Task GenerateStateItemHash_WithPropertiesReordered_ShouldReturnSameHashSumsAsync()
        {
            var payload = new
            {
                eventType = "Youforce.OrganizationalUnit.Single",
                eventBody = new
                {
                    Id = "1234567890101112131",
                    ShortName = "FAO1234",
                    FullName = "FAKEORG1234",
                    Active = true,
                    Version = new
                    {
                        ValidityPeriod = new
                        {
                            VersionValidFrom = "2019-07-01",
                            VersionValidTo = "9999-12-31"
                        },
                        Managers = new
                        {
                            PersonNumber = "4321"
                        }
                    }
                }
            };

            var payloadChangedOrder = new
            {
                eventType = "Youforce.OrganizationalUnit.Single",
                eventBody = new
                {
                    Version = new
                    {
                        ValidityPeriod = new
                        {
                            VersionValidFrom = "2019-07-01",
                            VersionValidTo = "9999-12-31"
                        },
                        Managers = new
                        {
                            PersonNumber = "4321"
                        }
                    },
                    Id = "1234567890101112131",
                    ShortName = "FAO1234",
                    FullName = "FAKEORG1234",
                    Active = true
                }
            };
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var httpRequestMessage = GenerateHttpPostRequestMessage(jsonPayload);
            var jsonPayloadChangedOrder = JsonConvert.SerializeObject(payloadChangedOrder);
            var httpRequestMessageChangedOrder = GenerateHttpPostRequestMessage(jsonPayloadChangedOrder);
            var response = await generateStateItemHash.Run(httpRequestMessage, logger) as ObjectResult;
            var responseChangedOrder = await generateStateItemHash.Run(httpRequestMessageChangedOrder, logger) as ObjectResult;
            Assert.Equal(response.Value.ToString(), responseChangedOrder.Value.ToString());
        }

        private static HttpRequestMessage GenerateHttpPostRequestMessage(string requestBody)
        {
            var requestMessage = new HttpRequestMessage()
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json"),
                Method = HttpMethod.Post
            };
            return requestMessage;
        }
    }
}
