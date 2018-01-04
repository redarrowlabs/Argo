using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RedArrow.Argo.Client.Config.Serialization
{
    public class DictionaryAsArrayConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return
                objectType != typeof(JObject)
                && objectType.GetTypeInfo().ImplementedInterfaces.Any(i =>
                    i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var kvpType = typeof(KeyValuePair<,>).MakeGenericType(
                objectType.GenericTypeArguments[0],
                objectType.GenericTypeArguments[1]);

            var listType = typeof(List<>).MakeGenericType(kvpType);

            var result = (IDictionary)Activator.CreateInstance(objectType);
            var items = serializer.Deserialize(reader, listType);
            foreach (var item in items as IEnumerable)
            {
                result.Add(
                    kvpType.GetTypeInfo().GetDeclaredProperty("Key").GetValue(item),
                    kvpType.GetTypeInfo().GetDeclaredProperty("Value").GetValue(item)
                );
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var listType = typeof(List<>).MakeGenericType(
                typeof(KeyValuePair<,>).MakeGenericType(
                    value.GetType().GenericTypeArguments[0],
                    value.GetType().GenericTypeArguments[1]
                ));

            var list = (IList)Activator.CreateInstance(listType);
            foreach (var item in value as IEnumerable)
            {
                list.Add(item);
            }

            serializer.Serialize(writer, list);
        }
    }
}
