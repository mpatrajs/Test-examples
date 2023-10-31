using System;
using Newtonsoft.Json.Linq;

namespace Functions.Shared
{
    public static class JTokenExtensions
    {
        public static JToken MergeWith(this JToken jToken, JToken tokenToMerge)
        {
            if (jToken.IsNullOrEmpty() && tokenToMerge.IsNullOrEmpty())
            {
                return null;
            }

            if (jToken.IsNullOrEmpty())
            {
                return tokenToMerge;
            }

            if (tokenToMerge.IsNullOrEmpty())
            {
                return jToken;
            }

            ( (JObject)jToken ).Merge(tokenToMerge, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Merge
            });

            return jToken;
        }

        public static bool IsNullOrEmpty(this JToken token) =>
                   ( token == null ) ||
                   ( token.Type == JTokenType.Array && !token.HasValues ) ||
                   ( token.Type == JTokenType.Object && !token.HasValues ) ||
                   ( token.Type == JTokenType.String && token.ToString() == string.Empty ) ||
                   ( token.Type == JTokenType.Null );

        public static void RemoveProperty(this JToken token, string path) => token.SelectToken(path).Parent.Remove();

        public static JToken ConvertWholeNumberFloatsToInteger(this JToken jToken)
        {
            if (jToken == null)
            {
                return null;
            }

            WalkNode(jToken, prop =>
            {
                if (prop.Value.Type == JTokenType.Float)
                {
                    var value = prop.Value.Value<float>();
                    if (value % 1 == 0)
                    {
                        prop.Value = prop.Value.Value<int>();
                    }
                }
            });
            return jToken;
        }

        public static void WalkNode(JToken node, Action<JProperty> propertyAction = null)
        {
            if (node == null)
            {
                return;
            }

            if (node.Type == JTokenType.Object)
            {
                foreach (var child in node.Children<JProperty>())
                {
                    propertyAction?.Invoke(child);
                    WalkNode(child.Value, propertyAction);
                }
            }
            else if (node.Type == JTokenType.Array)
            {
                foreach (var child in node.Children())
                {
                    WalkNode(child, propertyAction);
                }
            }
        }

        public static JToken SelectTokenCaseInsensitive(this JToken jToken, string path)
        {
            var jTokenCopy = jToken.DeepClone();
            var jTokenValue = ( (JObject)jTokenCopy ).GetValue(path, StringComparison.InvariantCultureIgnoreCase);
            return jTokenValue == null ? null : jTokenCopy.SelectToken(jTokenValue.Path);
        }
    }
}