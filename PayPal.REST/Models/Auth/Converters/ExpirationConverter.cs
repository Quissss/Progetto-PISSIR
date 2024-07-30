using System.Text.Json;
using System.Text.Json.Serialization;

namespace PayPal.REST.Models.Auth.Converters
{
    public class ExpirationConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var difference = reader.GetInt32();
            return DateTime.UtcNow.AddSeconds(difference);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
