using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PayPal.REST.Models.PaymentSources.Converters
{
    public class UserActionConverter : JsonConverter<UserAction>
    {
        public override UserAction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString()!;
            return value switch
            {
                "PAY_NOW" => UserAction.PayNow,
                "CONTINUE" => UserAction.Continue,
                _ => UserAction.PayNow
            };
        }

        public override void Write(Utf8JsonWriter writer, UserAction value, JsonSerializerOptions options)
        {
            string result = value switch
            {
                UserAction.PayNow => "PAY_NOW",
                UserAction.Continue => "CONTINUE",
                _ => "PAY_NOW"
            };
            writer.WriteStringValue(result);
        }
    }
}
