using PayPal.REST.Models.Orders.Converters;
using System.Text.Json.Serialization;

namespace PayPal.REST.Models.Orders;

public class PurchaseUnit
{
    [JsonPropertyName("reference_id")]
    public string ReferenceId { get; set; }
    [JsonPropertyName("items")]
    public List<Item> Items { get; set; }
    [JsonPropertyName("amount")]
    public Amount Amount { get; set; }
    [JsonPropertyName("payee")]
    public Payee Payee { get; set; }
}

public class Payee
{
    [JsonPropertyName("email_address")]
    public string EmailAddress { get; set; }

    [JsonPropertyName("merchant_id")]
    public string MerchantId { get; set; }
}

public class Item
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("quantity")]
    public string Quantity { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("category")]
    [JsonConverter(typeof(ItemCategoryConverter))]
    public ItemCategory Category { get; set; }

    [JsonPropertyName("unit_amount")]
    public EuroAmount UnitAmount { get; set; }
}

public class Amount : EuroAmount
{
    [JsonPropertyName("breakdown")]
    public Breakdown Breakdown { get; set; }
}

public class Breakdown
{
    [JsonPropertyName("item_total")]
    public EuroAmount ItemTotal { get; set; }
}

public class EuroAmount
{
    [JsonPropertyName("currency_code")]
    public string CurrencyCode => "EUR";

    [JsonPropertyName("value")]
    public string Value { get; set; }
}

public enum IntentType
{
    Capture,
    Authorize
}

public enum ItemCategory
{
    DigitalGoods,
    PhysicalGoods,
    Donation
}
