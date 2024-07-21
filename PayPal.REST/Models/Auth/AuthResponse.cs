using PayPal.REST.Models.Auth.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PayPal.REST.Models.Auth
{
    public class AuthResponse
    {
        [JsonPropertyName("scope")]
        [JsonConverter(typeof(ScopesConverter))]
        public List<string> Scopes { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("app_id")]
        public string AppId { get; set; }

        [JsonPropertyName("expires_in")]
        [JsonConverter(typeof(ExpirationConverter))]
        public DateTime Expires { get; set; }

        [JsonPropertyName("nonce")]
        public string Nonce { get; set; }

    }
}
