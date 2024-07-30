using System.Text.Json;
using System.Text.Json.Serialization;

namespace PayPal.REST.Models.PaymentSources.Converters
{
    public class PaymentMethodPreferenceConverter : JsonConverter<PaymentMethodPreference>
    {
        public override PaymentMethodPreference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString()!;
            return value switch
            {
                "IMMEDIATE_PAYMENT_REQUIRED" => PaymentMethodPreference.ImmediatePaymentRequired,
                "UNRESTRICTED" => PaymentMethodPreference.Unrestricted,
                _ => PaymentMethodPreference.ImmediatePaymentRequired
            };
        }

        public override void Write(Utf8JsonWriter writer, PaymentMethodPreference value, JsonSerializerOptions options)
        {
            string content = value switch
            {
                PaymentMethodPreference.ImmediatePaymentRequired => "IMMEDIATE_PAYMENT_REQUIRED",
                PaymentMethodPreference.Unrestricted => "UNRESTRICTED",
                _ => "IMMEDIATE_PAYMENT_REQUIRED"
            };
            writer.WriteStringValue(content);
        }
    }
}
