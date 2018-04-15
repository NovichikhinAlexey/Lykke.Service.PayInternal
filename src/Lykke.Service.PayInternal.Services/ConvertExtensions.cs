using System.Linq;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
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
    }
}
