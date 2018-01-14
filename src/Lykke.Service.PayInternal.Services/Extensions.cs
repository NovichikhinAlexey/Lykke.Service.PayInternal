using Lykke.Service.PayInternal.Core.Domain.Merchant;
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
    }
}
