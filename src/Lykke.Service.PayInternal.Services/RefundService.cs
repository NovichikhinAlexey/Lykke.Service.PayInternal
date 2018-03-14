using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QBitNinja.Client;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.Refund;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;
// ReSharper disable once RedundantUsingDirective
using NBitcoin;
using QBitNinja.Client.Models;

namespace Lykke.Service.PayInternal.Services
{
    [UsedImplicitly]
    public class 
        RefundService : IRefundService
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly QBitNinjaClient _qBitNinjaClient;
        private readonly IBtcTransferService _btcTransferService;
        private readonly ITransactionsService _transactionService;
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly IRefundRepository _refundRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly TimeSpan _expirationTime;
        private readonly IBlockchainTransactionRepository _transactionRepository;

        public RefundService(
            QBitNinjaClient qBitNinjaClient,
            IBtcTransferService btcTransferService,
            ITransactionsService transactionService,
            IPaymentRequestService paymentRequestService,
            IRefundRepository refundRepository,
            IWalletRepository walletRepository,
            IBlockchainTransactionRepository transactionRepository,
            TimeSpan expirationTime)
        {
            _qBitNinjaClient =
                qBitNinjaClient ?? throw new ArgumentNullException(nameof(qBitNinjaClient));
            _btcTransferService =
                btcTransferService ?? throw new ArgumentNullException(nameof(btcTransferService));
            _transactionService =
                transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _paymentRequestService =
                paymentRequestService ?? throw new ArgumentNullException(nameof(paymentRequestService));
            _refundRepository =
                refundRepository ?? throw new ArgumentNullException(nameof(refundRepository));
            _walletRepository =
                walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
            _transactionRepository =
                transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _expirationTime = expirationTime;
        }

        public async Task<IRefund> ExecuteAsync(IRefundRequest refund)
        {
            IPaymentRequest paymentRequest = await _paymentRequestService.FindAsync(refund.SourceAddress);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(refund.SourceAddress);

            if (paymentRequest.PaymentAssetId != LykkeConstants.BitcoinAssetId)
                throw new AssetNotSupportedException(paymentRequest.PaymentAssetId);

            if (!paymentRequest.MerchantId.Equals(refund.MerchantId))
                throw new PaymentRequestNotFoundException(refund.MerchantId, paymentRequest.Id);

            //todo: in some cases we can't procees if status was PaymentRequestStatus.Error
            if (paymentRequest.Status != PaymentRequestStatus.Confirmed &&
                paymentRequest.Status != PaymentRequestStatus.Error)
                throw new NotAllowedStatusException(paymentRequest.Status.ToString());

            var walletsCheckResult = await _checkupMerchantWallets(refund);

            if (!walletsCheckResult)
                throw new WalletNotFoundException(refund.SourceAddress);

            IEnumerable<IBlockchainTransaction> paymentRequestTxs =
                await _transactionRepository.GetByPaymentRequest(paymentRequest.Id);

            IEnumerable<IBlockchainTransaction> paymentTxs =
                paymentRequestTxs.Where(x => x.TransactionType == TransactionType.Payment).ToList();

            if (!paymentTxs.Any())
                throw new NoTransactionsToRefundException(paymentRequest.Id);

            if (paymentTxs.Count() > 1)
                throw new MultiTransactionRefundNotSupportedException(paymentTxs.Count());

            IBlockchainTransaction txToRefund = paymentTxs.First();

            if (string.IsNullOrWhiteSpace(refund.DestinationAddress))
            {
                if (!txToRefund.SourceWalletAddresses.Any())
                    throw new NoTransactionsToRefundException(paymentRequest.Id);

                if (txToRefund.SourceWalletAddresses.Length > 1)
                    throw new MultiTransactionRefundNotSupportedException(txToRefund.SourceWalletAddresses.Length);
            }
            else
            {
                if (!txToRefund.SourceWalletAddresses.Contains(refund.DestinationAddress))
                    throw new WalletNotFoundException(refund.DestinationAddress);
            }

            BalanceSummary balanceSummary =
                await _qBitNinjaClient.GetBalanceSummary(BitcoinAddress.Create(refund.SourceAddress));

            decimal spendableSatoshi = balanceSummary.Spendable.Amount.ToDecimal(MoneyUnit.Satoshi);

            //todo: take into account different assets 
            //todo: consider situation if we can make partial refund
            decimal satoshiToRefund = txToRefund.Amount;
            if (spendableSatoshi < satoshiToRefund)
                throw new NotEnoughMoneyException(spendableSatoshi, satoshiToRefund);

            var newRefund = new Refund
            {
                PaymentRequestId = paymentRequest.Id,
                DueDate = DateTime.UtcNow.Add(_expirationTime),
                MerchantId = refund.MerchantId,
                RefundId = Guid.NewGuid().ToString(),
                Amount = satoshiToRefund
                // TODO: what about settlement ID?
            };

            string destinationAddress = string.IsNullOrWhiteSpace(refund.DestinationAddress)
                ? txToRefund.SourceWalletAddresses.First()
                : refund.DestinationAddress;

            var newTransfer = new MultipartTransfer
            {
                PaymentRequestId = paymentRequest.Id,
                AssetId = paymentRequest.PaymentAssetId,
                CreationDate = DateTime.UtcNow,
                FeeRate = 0, // TODO: make sure this is correct
                FixedFee = (decimal)paymentRequest.MarkupFixedFee,
                MerchantId = refund.MerchantId,
                TransferId = Guid.NewGuid().ToString(),
                Parts = new List<TransferPart>
                {
                    new TransferPart
                    {
                        Destination = new AddressAmount
                        {
                            Address = destinationAddress,
                            Amount = satoshiToRefund
                        },
                        Sources = new List<AddressAmount>
                        {
                            new AddressAmount
                            {
                                Address = refund.SourceAddress,
                                Amount = satoshiToRefund
                            }
                        }
                    }
                }
            };

            await _refundRepository.AddAsync(newRefund);

            MultipartTransferResponse transferResponse =
                await _btcTransferService.ExecuteMultipartTransferAsync(newTransfer, TransactionType.Refund);

            if (transferResponse.State == TransferExecutionResult.Fail)
                throw new Exception(transferResponse.ErrorMessage);

            await _paymentRequestService.ProcessAsync(refund.SourceAddress);

            return new RefundResponse
            {
                MerchantId = refund.MerchantId,
                PaymentRequestId = paymentRequest.Id,
                RefundId = newRefund.RefundId,
                DueDate = newRefund.DueDate,
                Amount = satoshiToRefund
                // TODO: what about settlement ID?
            };
        }

        public async Task<IRefund> GetStateAsync(string merchantId, string refundId)
        {
            return await _refundRepository.GetAsync(merchantId, refundId);
        }

        private async Task<bool> _checkupMerchantWallets(IRefundRequest refund)
        {
            var wallets = (await _walletRepository.GetByMerchantAsync(refund.MerchantId))?.ToList();

            if (wallets == null || !wallets.Any()) return false;

            // Currently we check up only the source address. But is may be useful to check the destination either.
            return wallets.Any(w => w.Address == refund.SourceAddress);
        }
    }
}
