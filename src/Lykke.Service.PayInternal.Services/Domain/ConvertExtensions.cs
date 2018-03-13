using System.Collections.Generic;
using System.Globalization;
using Common;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Refund;
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

        public static RefundResponse ToApiModel(this IRefund src)
        {
            return new RefundResponse
            {
                Amount = src.Amount,
                MerchantId = src.MerchantId,
                PaymentRequestId = src.PaymentRequestId,
                RefundId = src.RefundId,
                SettlementId = src.SettlementId,
                DueDate = src.DueDate
            };
        }
    }
}
