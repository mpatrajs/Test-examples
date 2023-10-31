using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Functions.Shared
{
    public class JTokenDictionary
    {
        private readonly Dictionary<JToken, List<string>> _dict = new();

        public void Add(JToken jToken, string value)
        {
            if (!_dict.ContainsKey(jToken))
            {
                _dict[jToken] = new List<string>();
            }

            _dict[jToken].Add(value);
        }

        public List<string> GetValues(JToken jToken) => _dict[jToken];

        public void RemoveDuplicateValues()
        {
            var listItems = new List<string>();
            foreach (var values in _dict.Values)
            {
                listItems.AddRange(values);
            }

            var duplicateValues = listItems.GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key)
                .ToList();

            foreach (var values in _dict.Values)
            {
                foreach (var duplicateValue in duplicateValues)
                {
                    if (values.Any(v => v == duplicateValue))
                    {
                        values.Remove(duplicateValue);
                    }
                }
            }
        }

        public Dictionary<JToken, List<string>>.Enumerator GetEnumerator() => _dict.GetEnumerator();
    }
}