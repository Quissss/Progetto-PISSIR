using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PayPal.REST.Models.Orders.Converters
{
    public class IntentTypeConverter : JsonConverter<IntentType>
    {
        public override IntentType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (IntentType)Enum.Parse(typeToConvert, reader.GetString(), true);
        }

        public override void Write(Utf8JsonWriter writer, IntentType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString().ToUpperInvariant());
        }
    }
}
