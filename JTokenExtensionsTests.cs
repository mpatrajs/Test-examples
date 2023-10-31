using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Functions.Shared.UnitTests
{
    public class JTokenExtensionTests
    {
        [Fact]
        public void MergeWith_BothNull_ReturnsNull()
        {
            var mergeObj = new MergeContainer
            {
                Left = null,
                Right = null
            };
            var result = mergeObj.Left.MergeWith(mergeObj.Right);
            result.Should().BeNull();
        }

        [Fact]
        public void MergeWith_SamePropertyNull_ReturnsNullProperty()
        {
            var obj1 = JObject.Parse("{\"Id\": \"123\",\"LastName\":null,\"AccountName\":\"test@youforce.net\"}");
            var obj2 = JObject.Parse("{\"Id\": \"321\",\"LastName\":null}");

            var mergeObj = new MergeContainer
            {
                Left = obj1,
                Right = obj2
            };

            var result = mergeObj.Left.MergeWith(mergeObj.Right);
            result["Id"].Value<string>().Should().Be("321");
            result["LastName"].Value<string>().Should().BeNull();
            result.SelectToken("FirstName").Should().BeNull();
        }

        [Fact]
        public void MergeWith_RightIsNull_LeftJToken()
        {
            var obj1 = JObject.Parse("{\"Id\": \"123\",\"LastName\":null,\"AccountName\":\"test@youforce.net\"}");

            var mergeObj = new MergeContainer
            {
                Left = obj1,
                Right = null
            };

            var result = mergeObj.Left.MergeWith(mergeObj.Right);

            result.Should().BeEquivalentTo(mergeObj.Left);
        }

        [Fact]
        public void MergeWith_LeftIsNull_RightJToken()
        {
            var obj1 = JObject.Parse("{\"Id\": \"123\",\"LastName\":null}");

            var mergeObj = new MergeContainer
            {
                Left = null,
                Right = obj1
            };

            var result = mergeObj.Right.MergeWith(mergeObj.Left);

            result.Should().BeEquivalentTo(mergeObj.Right);
        }

        [Fact]
        public void MergeJson_Should_Include_Null_Values()
        {
            var obj1 = JObject.Parse(
                "{\"Id\": \"123\",\"LastName\":null,\"Contact\": {\"Mail\": \"testmail@youforce.net\"}}");
            var obj2 = JObject.Parse("{\"Id\": null, \"FirstName\":\"testname\", \"LastName\": \"testsurname\"}");

            var mergeObj = new MergeContainer
            {
                Left = obj1,
                Right = obj2
            };

            var result = mergeObj.Left.MergeWith(mergeObj.Right);
            result["Id"].Value<string>().Should().BeNull();
            result["LastName"].Value<string>().Should().Be("testsurname");
            result["Contact"]["Mail"].Value<string>().Should().Be("testmail@youforce.net");
        }

        [Fact]
        public void SelectTokenCaseInsensitive_NameDifferentCasing_ReturnsJToken()
        {
            var obj1 = JObject.Parse("{\"Id\": \"123\",\"LastName\":\"testname\",\"AccountName\":\"test@youforce.net\"}");
            var result = obj1.SelectTokenCaseInsensitive("lastname");
            result.Should().NotBeNull();
        }

        [Fact]
        public void SelectTokenCaseInsensitive_NonExistingNode_ReturnsNull()
        {
            var obj1 = JObject.Parse("{\"Id\": \"123\",\"LastName\":\"testname\",\"AccountName\":\"test@youforce.net\"}");
            var result = obj1.SelectTokenCaseInsensitive("nonExistingNode");
            result.Should().BeNull();
        }

        [Fact]
        public void ConvertWholeNumberFloatsToInteger_JTokenWithFloats_ExpectedResult()
        {
            var obj1 = JObject.Parse("{\"Id\": \"123\",\"WholeFloat\":2.0,\"RealFloat\":3.2, \"RealFloatAsString\":\"3.2\"}");
            var result = obj1.ConvertWholeNumberFloatsToInteger();
            ( (JObject)result ).GetValue("WholeFloat").Type.Should().Be(JTokenType.Integer);
            ( (JObject)result ).GetValue("RealFloat").Type.Should().Be(JTokenType.Float);
            ( (JObject)result ).GetValue("RealFloatAsString").Type.Should().Be(JTokenType.String);
        }
    }
}