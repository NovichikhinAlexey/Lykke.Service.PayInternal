using System.Collections.Generic;
using System.Linq;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Services.Domain;

namespace Lykke.Service.PayInternal.Services
{
    public static class ConvertExtensions
    {
        public static TransferTransactionResult ToResult(this TransferTransaction src)
        {
            return new TransferTransactionResult
            {
                Amount = src.Amount,
                AssetId = src.AssetId,
                Hash = src.Hash,
                Error = src.Error,
                Sources = src.Sources,
                Destinations = src.Destinations
            };
        }

        public static TransferResult ToResult(this ITransfer src)
        {
            return new TransferResult
            {
                Id = src.Id,
                Blockchain = src.Blockchain,
                Transactions = src.Transactions.Select(x => x.ToResult()),
                Timestamp = src.CreatedOn
            };
        }

        public static TransferTransaction ToTransfer(this BlockchainTransactionResult src)
        {
            return new TransferTransaction
            {
                Amount = src.Amount,
                AssetId = src.AssetId,
                Hash = src.Hash,
                Error = src.Error,
                Sources = src.Sources,
                Destinations = src.Destinations
            };
        }

        public static BlockchainTransferCommand ToBlockchainTransfer(this TransferCommand src)
        {
            return new BlockchainTransferCommand
            {
                AssetId = src.AssetId,
                Amounts = src.Amounts
            };
        }

        public static IMerchantMarkup GetMarkup(this IMerchant src)
        {
            return new MerchantMarkup
            {
                LpPercent = src.LpMarkupPercent,
                DeltaSpread = src.DeltaSpread,
                LpPips = src.LpMarkupPips,
                LpFixedFee = src.MarkupFixedFee
            };
        }

        public static ICreateTransaction ToDomain(this ICreateTransactionRequest src)
        {
            return new CreateTransaction
            {
                WalletAddress = src.WalletAddress,
                Amount = (decimal) src.Amount,
                FirstSeen = src.FirstSeen,
                Confirmations = src.Confirmations,
                BlockId = src.BlockId,
                TransactionId = src.TransactionId,
                Blockchain = src.Blockchain,
                AssetId = src.AssetId,
                SourceWalletAddresses = src.SourceWalletAddresses
            };
        }

        public static IUpdateTransaction ToDomain(this IUpdateTransactionRequest src)
        {
            return new UpdateTransaction
            {
                WalletAddress = src.WalletAddress,
                Amount = src.Amount,
                Confirmations = src.Confirmations,
                BlockId = src.BlockId,
                TransactionId = src.TransactionId,
                FirstSeen = src.FirstSeen
            };
        }

        public static TransferCommand ToRefundTransferCommand(this IPaymentRequestTransaction src, string destination = null)
        {
            return new TransferCommand
            {
                AssetId = src.AssetId,
                Amounts = new List<TransferAmount>
                {
                    new TransferAmount
                    {
                        Amount = src.Amount,
                        Source = src.WalletAddress,
                        Destination = string.IsNullOrWhiteSpace(destination)
                            ? src.SourceWalletAddresses.Single()
                            : destination,
                    }
                }
            };
        }
    }
}
