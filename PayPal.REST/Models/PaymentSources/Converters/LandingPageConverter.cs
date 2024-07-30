using System.Text.Json;
using System.Text.Json.Serialization;

namespace PayPal.REST.Models.PaymentSources.Converters
{
    public class LandingPageConverter : JsonConverter<LandingPage>
    {
        public override LandingPage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString()!;
            return value switch
            {
                "LOGIN" => LandingPage.Login,
                "GUEST_CHECKOUT" => LandingPage.GuestCheckout,
                "NO_PREFERENCE" => LandingPage.NoPreference,
                _ => LandingPage.NoPreference
            };
        }

        public override void Write(Utf8JsonWriter writer, LandingPage value, JsonSerializerOptions options)
        {
            string result = value switch
            {
                LandingPage.Login => "LOGIN",
                LandingPage.GuestCheckout => "GUEST_CHECKOUT",
                LandingPage.NoPreference => "NO_PREFERENCE",
                _ => "NO_PREFERENCE"
            };
            writer.WriteStringValue(result);
        }
    }
}
