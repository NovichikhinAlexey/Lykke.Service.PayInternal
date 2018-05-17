using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.PayInternal.Core.Domain.Orders;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class PaymentRequestDetailsBuilder : IPaymentRequestDetailsBuilder
    {
        private readonly IOrderService _orderService;
        private readonly ITransactionsService _transactionsService;

        public PaymentRequestDetailsBuilder(
            IOrderService orderService,
            ITransactionsService transactionsService)
        {
            _orderService = orderService;
            _transactionsService = transactionsService;
        }

        public async Task<TResult> Build<TResult, TOrder, TTransaction, TRefund>(IPaymentRequest paymentRequest,
            PaymentRequestRefund refundInfo)
        {
            IOrder order = await _orderService.GetAsync(paymentRequest.Id, paymentRequest.OrderId);

            IReadOnlyList<IPaymentRequestTransaction> transactions =
                await _transactionsService.GetByWalletAsync(paymentRequest.WalletAddress);

            IEnumerable<IPaymentRequestTransaction> paymentTransactions = transactions.Where(x => x.IsPayment());

            dynamic model = Mapper.Map<TResult>(paymentRequest);

            model.Order = Mapper.Map<TOrder>(order);

            model.Transactions = Mapper.Map<List<TTransaction>>(paymentTransactions);

            model.Refund = Mapper.Map<TRefund>(refundInfo);

            return model;
        }
    }
}
