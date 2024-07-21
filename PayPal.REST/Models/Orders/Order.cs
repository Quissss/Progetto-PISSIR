using PayPal.REST.Models.Orders.Converters;
using PayPal.REST.Models.PaymentSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
