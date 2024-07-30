using PayPal.REST.Models.PaymentSources.Converters;
using System.Text.Json.Serialization;

namespace PayPal.REST.Models.PaymentSources
{
    public class PayPalPaymentSource : IPaymentSource
    {
        [JsonPropertyName("paypal")]
        public PayPalData PayPalData { get; set; }
    }

    public class PayPalData
    {
        [JsonPropertyName("name")]
        public Name Name { get; set; }

        [JsonPropertyName("email_address")]
        public string EmailAddress { get; set; }

        [JsonPropertyName("experience_context")]
        public ExperienceContext ExperienceContext { get; set; }
    }

    public class ExperienceContext
    {
        [JsonPropertyName("payment_method_preference")]
        [JsonConverter(typeof(PaymentMethodPreferenceConverter))]
        public PaymentMethodPreference PaymentMethodPreference { get; set; }

        [JsonPropertyName("brand_name")]
        public string BrandName { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        [JsonPropertyName("landing_page")]
        [JsonConverter(typeof(LandingPageConverter))]
        public LandingPage LandingPage { get; set; }

        [JsonPropertyName("shipping_preference")]
        [JsonConverter(typeof(ShippingPreferenceConverter))]
        public ShippingPreference ShippingPreference { get; set; }

        [JsonPropertyName("user_action")]
        [JsonConverter(typeof(UserActionConverter))]
        public UserAction UserAction { get; set; }

        [JsonPropertyName("return_url")]
        public string ReturnUrl { get; set; }

        [JsonPropertyName("cancel_url")]
        public string CancelUrl { get; set; }
    }

    public class Name
    {
        [JsonPropertyName("given_name")]
        public string GivenName { get; set; }

        [JsonPropertyName("surname")]
        public string Surname { get; set; }
    }
}
