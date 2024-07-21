using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PayPal.REST.Models.Orders.Converters
{
    public class ItemCategoryConverter : JsonConverter<ItemCategory>
    {
        public override ItemCategory Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (ItemCategory)Enum.Parse(typeToConvert, reader.GetString().Replace("_", string.Empty), true);
        }

        public override void Write(Utf8JsonWriter writer, ItemCategory value, JsonSerializerOptions options)
        {
            string content = value switch
            {
                ItemCategory.PhysicalGoods => "PHYSICAL_GOODS",
                ItemCategory.DigitalGoods => "DIGITAL_GOODS",
                ItemCategory.Donation => "DONATION",
                _ => "DIGITAL_GOODS"
            };
            writer.WriteStringValue(content);
        }
    }
}
