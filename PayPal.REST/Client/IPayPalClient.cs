using PayPal.REST.Models.Orders;
using PayPal.REST.Models.PaymentSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace PayPal.REST.Client
{
    public interface IPayPalClient : IDisposable
    {
        public Task<OrderResponse> CreateOrder(OrderRequest request, CancellationToken token = default);

        public Task<OrderResponse> ConfirmOrder(string orderId, IPaymentSource paymentSource, CancellationToken token = default);

        public Task<OrderResponse> AuthorizePayment(string orderId, CancellationToken token = default);

        public Task<OrderResponse?> VoidAuthorization(string orderId, CancellationToken token = default);

        public Task<OrderResponse> CapturePayment(string orderId, CancellationToken token = default);

        public Task<OrderResponse> RefundPayment(string orderId, CancellationToken token = default);

        public Task<OrderResponse?> GetOrderById(string orderId, CancellationToken token = default);

    }
}
