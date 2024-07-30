using System.Text.Json;
using System.Text.Json.Serialization;

namespace PayPal.REST.Models.Orders.Converters
{
    public class OrderStatusConverter : JsonConverter<OrderStatus>
    {
        public override OrderStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString()!;
            return value switch
            {
                "PAYER_ACTION_REQUIRED" => OrderStatus.PayerActionRequired,
                _ => (OrderStatus)Enum.Parse(typeToConvert, value, true)
            };
        }

        public override void Write(Utf8JsonWriter writer, OrderStatus value, JsonSerializerOptions options)
        {
            string content = value.ToString().ToUpperInvariant();
            if (value == OrderStatus.PayerActionRequired)
                content = "PAYER_ACTION_REQUIRED";
            writer.WriteStringValue(content);
        }
    }
}
