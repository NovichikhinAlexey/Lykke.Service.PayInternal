using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.Assets.Client.Models;
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
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly ILog _log;
        private readonly int _transactionConfirmationCount;

        public PaymentRequestService(
            IPaymentRequestRepository paymentRequestRepository,
            IMerchantWalletsService merchantWalletsService,
            IBlockchainTransactionRepository transactionRepository,
            IOrderService orderService,
            IPaymentRequestPublisher paymentRequestPublisher,
            IAssetsLocalCache assetsLocalCache,
            ILog log,
            int transactionConfirmationCount)
        {
            _paymentRequestRepository = paymentRequestRepository;
            _merchantWalletsService = merchantWalletsService;
            _transactionRepository = transactionRepository;
            _orderService = orderService;
            _paymentRequestPublisher = paymentRequestPublisher;
            _assetsLocalCache = assetsLocalCache;
            _log = log;
            _transactionConfirmationCount = transactionConfirmationCount;
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

        public async Task ProcessAsync(string walletAddress)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.FindAsync(walletAddress);

            if(paymentRequest == null)
                throw new PaymentRequestNotFoundException(walletAddress);
            
            IReadOnlyList<IBlockchainTransaction> transactions = await _transactionRepository.GetAsync(walletAddress);

            if (paymentRequest.Status == PaymentRequestStatus.New)
                paymentRequest.Status = PaymentRequestStatus.InProcess;

            if (paymentRequest.Status == PaymentRequestStatus.InProcess && transactions.All(o=>o.Confirmations >= _transactionConfirmationCount))
            {
                DateTime paidDate = transactions.Max(o => o.FirstSeen);
                decimal sum = transactions.Sum(o => o.Amount) * (decimal) Math.Pow(10, -1 * 8);
                
                IOrder order = await _orderService.GetAsync(paymentRequest.Id, paidDate);

                if (order == null)
                {
                    paymentRequest.Status = PaymentRequestStatus.Error;
                    paymentRequest.Error = "EXPIRED";
                }
                else
                {
                    AssetPair assetPair = await _assetsLocalCache.GetAssetPairByIdAsync(order.AssetPairId);

                    decimal diff = sum - order.PaymentAmount;

                    if (Math.Abs(diff) <= (decimal) Math.Pow(10, -1 * assetPair.Accuracy))
                    {
                        paymentRequest.Status = PaymentRequestStatus.Confirmed;
                    }
                    else
                    {
                        paymentRequest.Status = PaymentRequestStatus.Error;
                        paymentRequest.Error = diff < 0 ? "AMOUNT BELOW" : "AMOUNT ABOVE";
                    }
                }

                paymentRequest.PaidAmount = sum;
                paymentRequest.PaidDate = paidDate;
            }

            await _paymentRequestRepository.UpdateAsync(paymentRequest);

            await _paymentRequestPublisher.PublishAsync(paymentRequest);
        }
    }
}
