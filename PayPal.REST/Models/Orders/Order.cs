using PayPal.REST.Models.Orders.Converters;
using PayPal.REST.Models.PaymentSources;
using System.Text.Json.Serialization;

namespace PayPal.REST.Models.Orders
{
    public class Order
    {
        [JsonPropertyName("purchase_units")]
        public List<PurchaseUnit> PurchaseUnits { get; set; }

        [JsonPropertyName("intent")]
        [JsonConverter(typeof(IntentTypeConverter))]
        public IntentType Intent { get; set; }

        [JsonPropertyName("payment_source")]
        public IPaymentSource PaymentSource { get; set; }
    }
}
