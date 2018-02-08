using Lykke.Service.PayInternal.Core.Domain.Merchant;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class MerchantMarkup : IMerchantMarkup
    {
        public double DeltaSpread { get; set; }
        public double LpPercent { get; set; }
        public double LpPips { get; set; }
        public double LpFixedFee { get; set; }
    }
}
