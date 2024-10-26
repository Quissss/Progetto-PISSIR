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

/// <summary>
/// Represents the PayPal client for handling API requests to the PayPal REST API.
/// </summary>
namespace PayPal.REST.Client
{
    public class PayPalClient : IPayPalClient
    {

        private AuthResponse? _auth = null;
        private readonly string _clientId = string.Empty;
        private readonly string _clientSecret = string.Empty;
        private readonly string _payPalUrl = "https://api.sandbox.paypal.com";
        private readonly HttpClient _client;

        private readonly JsonSerializerOptions _options;

        /// <summary>
        /// Constructor for <see cref="PayPalClient"/> using configuration options.
        /// </summary>
        /// <param name="options">Configuration options containing client credentials and PayPal URL.</param>
        public PayPalClient(IOptions<PayPalClientOptions> options) : this()
        {
            _clientId = options.Value.ClientId;
            _clientSecret = options.Value.ClientSecret;
            _client = new HttpClient() { BaseAddress = new(options.Value.PayPalUrl) };
            Configure();
        }

        /// <summary>
        /// Constructor for <see cref="PayPalClient"/> with explicit client credentials.
        /// </summary>
        /// <param name="clientId">The PayPal API client ID.</param>
        /// <param name="clientSecret">The PayPal API client secret.</param>
        public PayPalClient(string clientId, string clientSecret) : this()
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _client = new HttpClient() { BaseAddress = new(_payPalUrl) };
            Configure();
        }

        private PayPalClient()
        {
            _options = new();
            _options.Converters.Add(new TypeMappingConverter<IPaymentSource, PayPalPaymentSource>());
        }

        /// <summary>
        /// Configures the default request headers for the HTTP client.
        /// </summary>
        private void Configure()
        {

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Prefer", "return=representation");
        }

        /// <summary>
        /// Authorizes the client by obtaining a new access token if the current token is expired or not set.
        /// </summary>
        /// <param name="token">Cancellation token for the async operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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

        /// <summary>
        /// Creates a new PayPal order based on the provided order request.
        /// </summary>
        /// <param name="request">The <see cref="OrderRequest"/> containing order details.</param>
        /// <param name="token">Cancellation token for the async operation.</param>
        /// <returns>An <see cref="OrderResponse"/> with the details of the created order.</returns>
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

        /// <summary>
        /// Confirms the payment source for a specified order.
        /// </summary>
        /// <param name="orderId">The PayPal order ID.</param>
        /// <param name="source">The payment source to confirm.</param>
        /// <param name="token">Cancellation token for the async operation.</param>
        /// <returns>An <see cref="OrderResponse"/> with updated order details.</returns>
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

        /// <summary>
        /// Retrieves an order by its ID.
        /// </summary>
        /// <param name="orderId">The PayPal order ID.</param>
        /// <param name="token">Cancellation token for the async operation.</param>
        /// <returns>An <see cref="OrderResponse"/> with order details or null if not found.</returns>
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

        /// <summary>
        /// Approves a PayPal order with the provided address.
        /// </summary>
        /// <param name="orderId">The PayPal order ID.</param>
        /// <param name="address">The address to be associated with the order.</param>
        /// <param name="token">Cancellation token for the async operation.</param>
        /// <returns>An <see cref="OrderResponse"/> with approved order details.</returns>
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

        /// <summary>
        /// Authorizes payment for a specified order.
        /// </summary>
        /// <param name="orderId">The PayPal order ID.</param>
        /// <param name="token">Cancellation token for the async operation.</param>
        /// <returns>An <see cref="OrderResponse"/> with details of the authorized payment.</returns>
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

        /// <summary>
        /// Captures the payment for a specified order.
        /// </summary>
        /// <param name="orderId">The PayPal order ID.</param>
        /// <param name="token">Cancellation token for the async operation.</param>
        /// <returns>An <see cref="OrderResponse"/> with details of the captured payment.</returns>
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

        /// <summary>
        /// Voids an authorization for a specified authorization ID.
        /// </summary>
        /// <param name="authorizationId">The authorization ID to void.</param>
        /// <param name="token">Cancellation token for the async operation.</param>
        /// <returns>An <see cref="OrderResponse"/> or null if the authorization is not found.</returns>
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

        /// <summary>
        /// The current authorization response, containing an access token and expiry time.
        /// </summary>
        public Task<OrderResponse> RefundPayment(string orderId, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Disposes of the <see cref="HttpClient"/> resources used by the PayPal client.
        /// </summary>
        public void Dispose()
        {
            _client?.Dispose();
        }

    }
}