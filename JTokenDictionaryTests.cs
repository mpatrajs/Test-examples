using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Functions.Shared.UnitTests
{
    public class JTokenDictionaryTests
    {
        [Fact]
        public void RemoveDuplicateValues_DictionaryWithDuplicateValues_OnlyUniqeValuesInDictionary()
        {
            var leftJToken = JObject.Parse("{}");
            var rightJToken = JObject.Parse("{}");
            var dictionary = new JTokenDictionary();
            dictionary.Add(leftJToken, "a");
            dictionary.Add(leftJToken, "b");
            dictionary.Add(leftJToken, "c");
            dictionary.Add(rightJToken, "c");
            dictionary.Add(rightJToken, "d");
            dictionary.RemoveDuplicateValues();
            dictionary.GetValues(leftJToken).Count.Should().Be(2);
            dictionary.GetValues(rightJToken).Count.Should().Be(1);
            dictionary.GetValues(leftJToken).Contains("c").Should().BeFalse();
            dictionary.GetValues(rightJToken).Contains("c").Should().BeFalse();
        }
    }
}