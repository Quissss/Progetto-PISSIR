using PayPal.REST.Models.Orders.Converters;
using System.Text.Json.Serialization;

namespace PayPal.REST.Models.Orders;

public class OrderResponse : Order
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(OrderStatusConverter))]
    public OrderStatus Status { get; set; }

    [JsonPropertyName("create_time")]
    public DateTime CreateTime { get; set; }

    [JsonPropertyName("links")]
    public List<Link> Links { get; set; }
}

public class Link
{
    [JsonPropertyName("href")]
    public string Href { get; set; }

    [JsonPropertyName("rel")]
    public string Rel { get; set; }

    [JsonPropertyName("method")]
    public string Method { get; set; }

}

public enum OrderStatus
{
    Created,
    Saved,
    Approved,
    Voided,
    Completed,
    PayerActionRequired
}
