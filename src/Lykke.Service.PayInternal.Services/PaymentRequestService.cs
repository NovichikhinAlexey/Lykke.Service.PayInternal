using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;

namespace Lykke.Service.PayInternal.Services
{
    public class PaymentRequestService : IPaymentRequestService
    {
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly IMerchantWalletsService _merchantWalletsService;
        private readonly IBlockchainTransactionRepository _transactionRepository;
        private readonly IOrderService _orderService;
        private readonly IPaymentRequestPublisher _paymentRequestPublisher;
        private readonly ILog _log;

        public PaymentRequestService(
            IPaymentRequestRepository paymentRequestRepository,
            IMerchantWalletsService merchantWalletsService,
            IBlockchainTransactionRepository transactionRepository,
            IOrderService orderService,
            IPaymentRequestPublisher paymentRequestPublisher,
            ILog log)
        {
            _paymentRequestRepository = paymentRequestRepository;
            _merchantWalletsService = merchantWalletsService;
            _transactionRepository = transactionRepository;
            _orderService = orderService;
            _paymentRequestPublisher = paymentRequestPublisher;
            _log = log;
        }
        
        public async Task<IReadOnlyList<IPaymentRequest>> GetAsync(string merchantId)
        {
            return await _paymentRequestRepository.GetAsync(merchantId);
        }

        public async Task<IPaymentRequest> GetAsync(string merchantId, string paymentRequestId)
        {
            return await _paymentRequestRepository.GetAsync(merchantId, paymentRequestId);
        }

        public async Task<IPaymentRequest> CreateAsync(IPaymentRequest paymentRequest)
        {
            paymentRequest.Status = PaymentRequestStatus.New;
            paymentRequest.WalletAddress =
                await _merchantWalletsService.CreateAddress(new CreateWallet
                {
                    DueDate = paymentRequest.DueDate,
                    MerchantId = paymentRequest.MerchantId
                });

            IPaymentRequest createdPaymentRequest = await _paymentRequestRepository.InsertAsync(paymentRequest);

            await _log.WriteInfoAsync(nameof(PaymentRequestService), nameof(CreateAsync),
                paymentRequest.ToJson(), "Payment request created.");

            return createdPaymentRequest;
        }

        public async Task<IPaymentRequest> CheckoutAsync(string merchantId, string paymentRequestId)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.GetAsync(merchantId, paymentRequestId);

            if(paymentRequest == null)
                throw new PaymentRequestNotFoundException(merchantId, paymentRequestId);
            
            // Don't create new order if payment reqest status not new. 
            if (paymentRequest.Status != PaymentRequestStatus.New)
                return paymentRequest;

            IOrder order = await _orderService.GetLatestOrCreateAsync(paymentRequest);

            if (paymentRequest.OrderId != order.Id)
            {
                paymentRequest.OrderId = order.Id;
                await _paymentRequestRepository.UpdateAsync(paymentRequest);

                await _log.WriteInfoAsync(nameof(PaymentRequestService), nameof(CheckoutAsync),
                    paymentRequest.ToJson(), "Payment request order updated.");
            }

            return paymentRequest;
        }

        // TODO: Rewrite
        public async Task ProcessAsync(string walletAddress)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.FindAsync(walletAddress);

            IReadOnlyList<IBlockchainTransaction> transactions =
                (await _transactionRepository.GetByWallet(walletAddress)).ToList();

            if (paymentRequest.Status == PaymentRequestStatus.New)
            {
                paymentRequest.Status = PaymentRequestStatus.InProcess;
            }

            if (paymentRequest.Status == PaymentRequestStatus.InProcess && transactions.All(o=>o.Confirmations >= 3))
            {
                paymentRequest.Status = PaymentRequestStatus.Confirmed;
                paymentRequest.PaidAmount = transactions.Sum(o => o.Amount);
                paymentRequest.PaidDate = transactions.OrderByDescending(o => o.FirstSeen).First().FirstSeen;
            }

            await _paymentRequestRepository.UpdateAsync(paymentRequest);

            await _paymentRequestPublisher.PublishAsync(paymentRequest);
        }
    }
}
