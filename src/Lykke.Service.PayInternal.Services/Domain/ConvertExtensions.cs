using System.Collections.Generic;
using System.Globalization;
using Common;
using Lykke.Service.PayInternal.Core.Domain.Merchant;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public static class ConvertExtensions
    {
        public static string ToContext(this IMerchant merchant)
        {
            return new Dictionary<string, string>
            {
                {nameof(merchant.Id), merchant.Id},
                {nameof(merchant.Name), merchant.Name},
                {nameof(merchant.TimeCacheRates), merchant.TimeCacheRates.ToString(CultureInfo.InvariantCulture)},
                {nameof(merchant.LwId), merchant.LwId}
            }.ToJson();
        }
    }
}
