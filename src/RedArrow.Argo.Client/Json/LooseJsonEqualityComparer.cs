using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using JTokenType = Newtonsoft.Json.Linq.JTokenType;

namespace RedArrow.Argo.Client.Json
{
    internal class LooseJsonEqualityComparer : IEqualityComparer<JToken>
    {
        public bool Equals(JToken token1, JToken token2)
        {
            /* Cannot use JToken.DeepEquals because a JSON string converted to JToken
             * will set most JTokenTypes to String while an object converted to JToken will
             * set JTokenTypes to the most specific type, like Guid, Uri, DateTime
             * or whatever so the DeepEquals always returns false. */
            
            // Null is null
            if (token1 == null || token1.Type == JTokenType.Null)
            {
                return token2 == null || token2.Type == JTokenType.Null;
            }
            if (token2 == null || token2.Type == JTokenType.Null)
            {
                return false;
            }
            
            if (token1.Type == JTokenType.Object 
                || token1.Type == JTokenType.Array 
                || token1.Type == JTokenType.Boolean
                || token1.Type == JTokenType.Property)
            {
                // These types will match in both a string and object that were converted to JTokens
                if (token1.Type != token2.Type)
                {
                    return false;
                }
            }
            else if (token1.Type == JTokenType.Integer || token1.Type == JTokenType.Float)
            {
                // If the json value is numeric, then it could be converted to either one of the numberic types
                if (token2.Type != JTokenType.Integer && token2.Type != JTokenType.Float)
                {
                    return false;
                }
            }
            // Else the type is too ambiguous to properly compare, like String and DateTime 
            //   are identically represented in JSON strings

            if (token1.Type == JTokenType.Array)
            {
                var jarray1 = (JArray)token1;
                var jarray2 = (JArray)token2;
                return jarray1.Count == jarray2.Count
                       && token1.Zip(token2, Equals).All(x => x);
            }
            if (token1.Type == JTokenType.Object)
            {
                var properties1 = ((JObject) token1).Properties().OrderBy(x => x.Name).ToArray();
                var properties2 = ((JObject) token2).Properties().OrderBy(x => x.Name).ToArray();
                return properties1.Length == properties2.Length
                       && properties1.Zip(properties2, Equals).All(x => x);
            }
            if (token1.Type == JTokenType.Property)
            {
                return Equals(((JProperty) token1).Value, ((JProperty) token2).Value);
            }

            if (token1.Type == token2.Type)
            {
                return token1.Value<object>().Equals(token2.Value<object>());
            }

            // Avoid weird situations like a DateTime != String for the same value
            return JsonConvert.SerializeObject(token1) == JsonConvert.SerializeObject(token2);
        }

        public int GetHashCode(JToken obj)
        {
            throw new NotImplementedException();
        }
    }
}
