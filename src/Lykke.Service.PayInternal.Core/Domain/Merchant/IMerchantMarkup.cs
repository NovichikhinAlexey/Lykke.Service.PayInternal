namespace Lykke.Service.PayInternal.Core.Domain.Merchant
{
    public interface IMerchantMarkup
    {
        double DeltaSpread { get; set; }

        double LpPercent { get; set; }

        double LpPips { get; set; }

        double LpFixedFee { get; set; }
    }
}
