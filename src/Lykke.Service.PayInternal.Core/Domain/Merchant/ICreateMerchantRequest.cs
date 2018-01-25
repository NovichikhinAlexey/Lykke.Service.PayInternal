namespace Lykke.Service.PayInternal.Core.Domain.Merchant
{
    public interface ICreateMerchantRequest
    {
        string Name { get; set; }

        string ApiKey { get; set; }

        double DeltaSpread { get; set; }

        int TimeCacheRates { get; set; }

        double LpMarkupPercent { get; set; }

        int LpMarkupPips { get; set; }

        string LwId { get; set; }
    }
}
