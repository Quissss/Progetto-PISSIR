using System.Text.Json;
using System.Text.Json.Serialization;

namespace PayPal.REST.Models.Auth.Converters
{
    public class ScopesConverter : JsonConverter<List<string>>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(List<string>);
        }

        public override List<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var strings = reader.GetString();
            return strings?.Split(" ").ToList();
        }

        public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
