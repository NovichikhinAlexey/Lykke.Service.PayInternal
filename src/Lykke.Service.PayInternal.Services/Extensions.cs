using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Services.Domain;

namespace Lykke.Service.PayInternal.Services
{
    public static class Extensions
    {
        public static IMerchantMarkup GetMarkup(this IMerchant src)
        {
            return new MerchantMarkup
            {
                LpPercent = src.LpMarkupPercent,
                DeltaSpread = src.DeltaSpread,
                LpPips = src.LpMarkupPips
            };
        }

        public static ICreateTransaction ToDomain(this ICreateTransactionRequest src)
        {
            return new CreateTransaction
            {
                WalletAddress = src.WalletAddress,
                Amount = src.Amount,
                FirstSeen = src.FirstSeen,
                Confirmations = src.Confirmations,
                BlockId = src.BlockId,
                TransactionId = src.TransactionId,
                Blockchain = src.Blockchain,
                AssetId = src.AssetId
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
                TransactionId = src.TransactionId
            };
        }
    }
}

