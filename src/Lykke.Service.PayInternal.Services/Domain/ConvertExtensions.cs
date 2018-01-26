using System.Collections.Generic;
using System.Globalization;
using Common;
using Lykke.Service.PayInternal.Contract;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Wallet;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public static class ConvertExtensions
    {
        public static CreateWallet ToDomain(this ICreateWalletRequest src)
        {
            return new CreateWallet
            {
                DueDate = src.DueDate,
                MerchantId = src.MerchantId
            };
        }

        public static NewWalletMessage ToNewWalletMessage(this IWallet src)
        {
            return new NewWalletMessage
            {
                Address = src.Address,
                DueDate = src.DueDate
            };
        }

        public static TransactionUpdateMessage ToMessage(this IBlockchainTransaction src)
        {
            return new TransactionUpdateMessage
            {
                Id = src.Id,
                WalletAddresss = src.WalletAddress,
                OrderId = src.OrderId,
                Amount = src.Amount,
                BlockId = src.BlockId,
                Confirmations = src.Confirmations
            };
        }

        public static string ToContext(this IMerchant merchant)
        {
            return new Dictionary<string, string>
            {
                {nameof(merchant.Id), merchant.Id},
                {nameof(merchant.Name), merchant.Name},
                {nameof(merchant.DeltaSpread), merchant.DeltaSpread.ToString(CultureInfo.InvariantCulture)},
                {nameof(merchant.TimeCacheRates), merchant.TimeCacheRates.ToString(CultureInfo.InvariantCulture)},
                {nameof(merchant.LpMarkupPercent), merchant.LpMarkupPercent.ToString(CultureInfo.InvariantCulture)},
                {nameof(merchant.LpMarkupPips), merchant.LpMarkupPips.ToString(CultureInfo.InvariantCulture)},
                {nameof(merchant.LwId), merchant.LwId}
            }.ToJson();
        }
    }
}
