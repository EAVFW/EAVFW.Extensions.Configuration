using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace EAVFW.Extensions.Configuration.RJSF
{

    public class UISchemaConverter : JsonConverter<UISchema>
    {
        public override UISchema ReadJson(JsonReader reader, Type objectType, UISchema existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var obj = new UISchema();
                reader.Read();
                while (reader.TokenType != JsonToken.EndObject)
                {

                    var name = reader.Value?.ToString();
                    reader.Read();
                    obj[name] = serializer.Deserialize<UISchema>(reader);
                    reader.Read();
                }
                return obj;
            }

            return new UISchema() { Value = serializer.Deserialize(reader) };

        }

        public override void WriteJson(JsonWriter writer, UISchema value, JsonSerializer serializer)
        {
            if (value.Value != null)
            {
                serializer.Serialize(writer, value.Value);
                return;
            }

            writer.WriteStartObject();
            foreach (var prop in value)
            {
                writer.WritePropertyName(prop.Key);
                serializer.Serialize(writer, prop.Value);
            }
            writer.WriteEndObject();
        }
    }

}
