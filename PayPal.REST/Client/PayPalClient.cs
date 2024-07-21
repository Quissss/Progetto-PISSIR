using Microsoft.Extensions.Options;
using PayPal.REST.Client.Converters;
using PayPal.REST.Models;
using PayPal.REST.Models.Auth;
using PayPal.REST.Models.Orders;
using PayPal.REST.Models.PaymentSources;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PayPal.REST.Client
{
    public class PayPalClient : IPayPalClient
    {

        private AuthResponse? _auth = null;
        private readonly string _clientId = string.Empty;
        private readonly string _clientSecret = string.Empty;
        private readonly string _payPalUrl = string.Empty;
        private readonly HttpClient _client;

        private readonly JsonSerializerOptions _options;

        public PayPalClient(IOptions<PayPalClientOptions> options) : this()
        {
            _clientId = options.Value.ClientId;
            _clientSecret = options.Value.ClientSecret;
            _client = new HttpClient() { BaseAddress = new(options.Value.PayPalUrl) };
            Configure();
        }

        public PayPalClient(string clientId, string clientSecret, string payPalUrl = "https://api.paypal.com") : this()
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _client = new HttpClient() { BaseAddress = new(payPalUrl) };
            Configure();
        }

        private PayPalClient()
        {            
            _options = new();
            _options.Converters.Add(new TypeMappingConverter<IPaymentSource, PayPalPaymentSource>());
        }

        private void Configure()        {
            
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Prefer", "return=representation");
        }

        private async Task AuthorizeClient(CancellationToken token = default)
        {
            if (_auth == null || _auth?.Expires < DateTime.UtcNow)
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/oauth2/token")
                {
                    Headers =
                    {
                        Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}")))
                    },
                    Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>() { new("grant_type", "client_credentials") }),
                };
                var res = await _client.SendAsync(httpRequest, token);
                res.EnsureSuccessStatusCode();

                _auth = await JsonSerializer.DeserializeAsync<AuthResponse>(await res.Content.ReadAsStreamAsync(token), cancellationToken: token);
            }
        }

        public async Task<OrderResponse> CreateOrder(OrderRequest request, CancellationToken token = default)
        {
            await AuthorizeClient(token);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v2/checkout/orders")
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", _auth!.AccessToken),
                },
                Content = JsonContent.Create(request),
            };

            var res = await _client.SendAsync(httpRequest, token);
            res.EnsureSuccessStatusCode();

            string json = await res.Content.ReadAsStringAsync(token);
            var order = JsonSerializer.Deserialize<OrderResponse>(json);

            return order;
        }

        public async Task<OrderResponse> ConfirmOrder(string orderId, IPaymentSource source, CancellationToken token = default)
        {
            await AuthorizeClient(token);
            var order = await GetOrderById(orderId, token);

            //if (order.PaymentSource is not null)
            //    return await ApproveOrder(orderId, token); 

            order!.PaymentSource = source;
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/v2/checkout/orders/{orderId}/confirm-payment-source")
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", _auth!.AccessToken),
                },
                Content = JsonContent.Create(order, options: _options),
            };

            var res = await _client.SendAsync(httpRequest, token);
            res.EnsureSuccessStatusCode();

            return (await JsonSerializer.DeserializeAsync<OrderResponse>(await res.Content.ReadAsStreamAsync(token), options: _options, token))!;
        }

        public async Task<OrderResponse?> GetOrderById(string orderId, CancellationToken token = default)
        {
            await AuthorizeClient(token);
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/v2/checkout/orders/{orderId}")
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", _auth!.AccessToken)
                }
            };

            var orderResponse = await _client.SendAsync(httpRequest, token);
            orderResponse.EnsureSuccessStatusCode();
            return await JsonSerializer.DeserializeAsync<OrderResponse>(await orderResponse.Content.ReadAsStreamAsync(token), _options, cancellationToken: token);
        }

        public async Task<OrderResponse> ApproveOrder(string orderId, IAddress address, CancellationToken token = default)
        {
            await AuthorizeClient(token);
            var order = await GetOrderById(orderId, token);

            dynamic edit = new ExpandoObject();
            edit.op = "replace";

            var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/v2/checkout/orders/{orderId}")
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", _auth!.AccessToken)
                },
                Content = JsonContent.Create(edit)
            };

            return order;
        }

        public async Task<OrderResponse> AuthorizePayment(string orderId, CancellationToken token = default)
        {
            await AuthorizeClient(token);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/v2/checkout/orders/{orderId}/authorize")
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", _auth!.AccessToken),
                },
                Content = new StringContent("", new MediaTypeHeaderValue("application/json"))
            };
            var res = await _client.SendAsync(httpRequest, token);
            res.EnsureSuccessStatusCode();
            return (await JsonSerializer.DeserializeAsync<OrderResponse>(await res.Content.ReadAsStreamAsync(), _options, token))!;
        }

        public async Task<OrderResponse> CapturePayment(string orderId, CancellationToken token = default)
        {
            await AuthorizeClient(token);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/v2/checkout/orders/{orderId}/capture")
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", _auth!.AccessToken),
                },
                Content = new StringContent("", new MediaTypeHeaderValue("application/json"))
            };
            var res = await _client.SendAsync(httpRequest, token);
            res.EnsureSuccessStatusCode();

            return (await JsonSerializer.DeserializeAsync<OrderResponse>(await res.Content.ReadAsStreamAsync(), options: _options, token))!;
        }
        public async Task<OrderResponse?> VoidAuthorization(string authorizationId, CancellationToken token = default)
        {
            await AuthorizeClient(token);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/v2/payments/authorizations/{authorizationId}/void")
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", _auth!.AccessToken),
                },
                Content = new StringContent("", new MediaTypeHeaderValue("application/json"))
            };
            var res = await _client.SendAsync(httpRequest, token);
            res.EnsureSuccessStatusCode();

            return (await JsonSerializer.DeserializeAsync<OrderResponse?>(await res.Content.ReadAsStreamAsync(), options: _options, token))!;
        }

        public Task<OrderResponse> RefundPayment(string orderId, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

    }
}
