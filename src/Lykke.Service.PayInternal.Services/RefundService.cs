using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain.Refund;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    [UsedImplicitly]
    public class RefundService : IRefundService
    {
        private readonly ITransferService _transferService;
        private readonly ITransactionsService _transactionService;
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly IRefundRepository _refundRepository;
        private readonly ILog _log;

        public RefundService(
            ITransferService transferService,
            ITransactionsService transactionService,
            IPaymentRequestService paymentRequestService,
            IRefundRepository refundRepository,
            ILog log)
        {
            _transferService =
                transferService ?? throw new ArgumentNullException(nameof(transferService));
            _transactionService =
                transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _paymentRequestService =
                paymentRequestService ?? throw new ArgumentNullException(nameof(paymentRequestService));
            _refundRepository =
                refundRepository ?? throw new ArgumentNullException(nameof(refundRepository));
            _log =
                log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<IRefund> ExecuteAsync(IRefundRequest refund)
        {
            var paymentRequest = await _paymentRequestService.FindAsync(refund.SourceAddress);
            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException("The payment request for the specified wallet address does not exist.");

            var transactions = await _transactionService.GetAsync(refund.SourceAddress);
            if (transactions == null)
                throw new TransactionNotFoundException("There are (still) no transactions for the payment request with the specified wallet address.");
            
            foreach (var tran in transactions)
            {
                // todo: 
                // 1. parse each transaction by BTC ninja lib;
                // 2. create a set of transfers for each parsed transaction (reverse the transactions itself) and using destionation address from the request (if specified);
                // 3. fullfill the resulting RefundResponse object to return;
                // 4. add appropriate logging of errors.
            }

            return new RefundResponse();
        }

        public async Task<IRefund> GetStateAsync(string merchantId, string refundId)
        {
            return await _refundRepository.GetAsync(merchantId, refundId);
        }
    }
}
