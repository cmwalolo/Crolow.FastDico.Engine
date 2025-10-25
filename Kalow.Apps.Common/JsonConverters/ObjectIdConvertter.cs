using Kalow.Apps.Common.DataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Kalow.Apps.Common.JsonConverters
{
    public class KalowIdConverter : JsonConverter
    {

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var token = JToken.Load(reader);
            //if (!(token is JValue))
            //    throw new JsonSerializationException("Token was not a primitive");
            if (token is JValue)
            {
                return new KalowId(token.Value<string>());
            }
            else
            {
                return KalowId.Empty;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(KalowId); //.IsAssignableFrom(objectType);
            //return true;
        }


    }
}
