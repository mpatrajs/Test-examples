using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace Common.Functions.UnitTests
{
    public class MergeJsonTests
    {
        [Fact]
        public void MergeJson_Should_Merge_Two_Json_Objects()
        {
            var loggerMock = Substitute.For<ILogger>();
            var obj1 = new
            {
                Id = "123",
                AccountName = "test@youforce.net",
                Contact = new
                {
                    workPhone1 = "0031612345678"
                }
            };
            var obj2 = new
            {
                Id = "1234",
                FirstName = "testname",
                LastName = "testsurname"
            };
            var mergeObj = new
            {
                Left = obj1,
                Right = obj2
            };
            var requestMessage = GenerateHttpPostRequestMessage(JsonConvert.SerializeObject(mergeObj));
            var response = MergeJson.Run(requestMessage, loggerMock) as ObjectResult;

            var employeeResult = response.Value.As<JObject>();

            Assert.IsType<OkObjectResult>(response);
            employeeResult["Id"].Value<string>().Should().Be("1234");
            employeeResult["AccountName"].Value<string>().Should().Be("test@youforce.net");
            employeeResult["FirstName"].Value<string>().Should().Be("testname");
            employeeResult["LastName"].Value<string>().Should().Be("testsurname");
            employeeResult["Contact"]["workPhone1"].Value<string>().Should().Be("0031612345678");
        }

        [Fact]
        public void MergeJson_Should_Return_Left_When_Right_Null()
        {
            var loggerMock = Substitute.For<ILogger>();
            var obj1 = new
            {
                Id = "123",
                AccountName = "test@youforce.net",
                Contact = new
                {
                    workPhone1 = "0031612345678"
                }
            };
            var mergeObj = new MergeObject
            {
                Left = obj1,
                Right = null
            };

            var requestMessage = GenerateHttpPostRequestMessage(JsonConvert.SerializeObject(mergeObj));
            var response = MergeJson.Run(requestMessage, loggerMock) as ObjectResult;

            var employeeResult = response.Value.As<JObject>();

            Assert.IsType<OkObjectResult>(response);
            employeeResult["Id"].Value<string>().Should().Be("123");
            employeeResult["AccountName"].Value<string>().Should().Be("test@youforce.net");
            employeeResult["Contact"]["workPhone1"].Value<string>().Should().Be("0031612345678");
        }

        [Fact]
        public void MergeJson_SamePropertyNull_ReturnsNullProperty()
        {
            var loggerMock = Substitute.For<ILogger>();
            var obj1 = JObject.Parse("{\"Id\": \"123\",\"LastName\":null,\"AccountName\":\"test@youforce.net\"}");
            var obj2 = JObject.Parse("{\"Id\": \"321\",\"LastName\":null}");

            var mergeObj = new JObjectMerge
            {
                Left = obj1,
                Right = obj2
            };
            var requestMessage = GenerateHttpPostRequestMessage(JsonConvert.SerializeObject(mergeObj));
            var response = MergeJson.Run(requestMessage, loggerMock) as ObjectResult;

            var employeeResult = response.Value.As<JObject>();

            Assert.IsType<OkObjectResult>(response);
            employeeResult["Id"].Value<string>().Should().Be("321");
            employeeResult["LastName"].Value<string>().Should().BeNull();
        }

        [Fact]
        public void MergeJson_BothNull_ReturnsNull()
        {
            var loggerMock = Substitute.For<ILogger>();

            var mergeObj = new JObjectMerge
            {
                Left = null,
                Right = null
            };

            var requestMessage = GenerateHttpPostRequestMessage(JsonConvert.SerializeObject(mergeObj));
            var response = MergeJson.Run(requestMessage, loggerMock) as ObjectResult;

            Assert.IsType<OkObjectResult>(response);
        }

        [Fact]
        public void MergeJson_Should_Include_Null_Values()
        {
            var loggerMock = Substitute.For<ILogger>();
            var obj1 = new
            {
                Id = "123",
            };
            var obj2 = new
            {
                FirstName = "testname",
                Contact = new
                {
                    workPhone1 = "0031612345678"
                }
            };
            var obj2Copy = JObject.FromObject(obj2);
            obj2Copy["Id"] = null;

            var mergeObj = new MergeObject
            {
                Left = obj1,
                Right = obj2Copy
            };

            var requestMessage = GenerateHttpPostRequestMessage(JsonConvert.SerializeObject(mergeObj));
            var response = MergeJson.Run(requestMessage, loggerMock) as ObjectResult;

            var employeeResult = response.Value.As<JObject>();

            Assert.IsType<OkObjectResult>(response);
            employeeResult["Id"].Value<string>().Should().BeNull();
            employeeResult["FirstName"].Value<string>().Should().Be("testname");
            employeeResult["Contact"]["workPhone1"].Value<string>().Should().Be("0031612345678");
        }

        [Fact]
        public void MergeJson_WithDifferentContract_ShouldOverwriteContract()
        {
            var loggerMock = Substitute.For<ILogger>();
            var obj1 = new
            {
                Contract = new
                {
                    Id = "731",
                    HireDate = "01-01-2018",
                    DisChargeDate = "01-07-2018",
                    Location = new
                    {
                        ShortName = "Henk"
                    }
                }
            };
            var obj2 = new
            {
                LastName = "testsurname",
                Contract = new
                {
                    Id = "732",
                    HireDate = "01-08-2018",
                    Location = new
                    {
                        ShortName = "Heiloo"
                    }
                }
            };
            var obj2Copy = JObject.FromObject(obj2);
            obj2Copy["Contract"]["DischargeDate"] = null;

            var mergeObj = new
            {
                Left = obj1,
                Right = obj2Copy
            };
            var requestMessage = GenerateHttpPostRequestMessage(JsonConvert.SerializeObject(mergeObj));
            var response = MergeJson.Run(requestMessage, loggerMock) as ObjectResult;

            var employeeResult = response.Value.As<JObject>();

            Assert.IsType<OkObjectResult>(response);

            employeeResult["LastName"].Value<string>().Should().Be("testsurname");
            employeeResult["Contract"]["DischargeDate"].Value<string>().Should().BeNull();
        }

        [Fact]
        public void MergeJson_WithDifferentManagerOf_ShouldKeepRightArray()
        {
            var loggerMock = Substitute.For<ILogger>();
            var obj1 = new
            {
                Id = "1337",
                HireDate = "14-04-2020",
                ManagerOf = new[]
                {
                    new { FullName = "Vasya", ShortName = "1" },
                    new { FullName = "Kostya", ShortName = "2" },
                    new { FullName = "Artur", ShortName = "3" }
                }
            };
            var obj2 = new
            {
                Id = "1337",
                HireDate = "14-04-2020",
                ManagerOf = new[]
                {
                    new { FullName = "Vasya", ShortName = "1" },
                    new { FullName = "Sanya", ShortName = "420" }
                }
            };
            var mergeObj = new
            {
                Left = obj1,
                Right = obj2
            };
            var requestMessage = GenerateHttpPostRequestMessage(JsonConvert.SerializeObject(mergeObj));
            var response = MergeJson.Run(requestMessage, loggerMock) as ObjectResult;
            var employeeResult = response.Value.As<JObject>();
            Assert.IsType<OkObjectResult>(response);
            employeeResult["ManagerOf"].Value<JArray>().Should().HaveCount(2);
            employeeResult["ManagerOf"][0]["FullName"].Value<string>().Should().Be("Vasya");
            employeeResult["ManagerOf"][0]["ShortName"].Value<string>().Should().Be("1");
            employeeResult["ManagerOf"][1]["FullName"].Value<string>().Should().Be("Sanya");
            employeeResult["ManagerOf"][1]["ShortName"].Value<string>().Should().Be("420");
        }

        private static DefaultHttpRequest GenerateHttpPostRequestMessage(string partialSubscriptionsObject)
        {
            var byteArray = Encoding.UTF8.GetBytes(partialSubscriptionsObject);
            var stream = new MemoryStream(byteArray);
            var requestMessage = new DefaultHttpRequest(new DefaultHttpContext())
            {
                ContentType = "application/json",
                Body = stream
            };
            return requestMessage;
        }
    }
    public class JObjectMerge
    {
        public JObject Left { get; set; }
        public JObject Right { get; set; }
    }

    public class MergeObject
    {
        public object Left { get; set; }
        public object Right { get; set; }
    }
}