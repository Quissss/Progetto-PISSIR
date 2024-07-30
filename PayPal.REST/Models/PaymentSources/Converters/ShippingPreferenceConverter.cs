using System.Text.Json;
using System.Text.Json.Serialization;

namespace PayPal.REST.Models.PaymentSources.Converters
{
    public class ShippingPreferenceConverter : JsonConverter<ShippingPreference>
    {
        public override ShippingPreference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString()!;
            return value switch
            {
                "GET_FROM_FILE" => ShippingPreference.GetFromFile,
                "NO_SHIPPING" => ShippingPreference.NoShipping,
                "SET_PROVIDED_ADDRESS" => ShippingPreference.SetProvidedAddress,
                _ => ShippingPreference.NoShipping
            };
        }

        public override void Write(Utf8JsonWriter writer, ShippingPreference value, JsonSerializerOptions options)
        {
            string result = value switch
            {
                ShippingPreference.GetFromFile => "GET_FROM_FILE",
                ShippingPreference.NoShipping => "NO_SHIPPING",
                ShippingPreference.SetProvidedAddress => "SET_PROVIDED_ADDRESS",
                _ => "NO_SHIPPING"
            };
            writer.WriteStringValue(result);
        }
    }
}
