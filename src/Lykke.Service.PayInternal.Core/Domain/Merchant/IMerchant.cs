namespace Lykke.Service.PayInternal.Core.Domain.Merchant
{
    public interface IMerchant
    {
        string Id { get; }

        string Name { get; set; }

        string PublicKey { get; set; }

        string ApiKey { get; set; }

        string LykkeWalletKey { get; set; }

        double DeltaSpread { get; set; }

        int TimeCacheRates { get; set; }

        double LpMarkupPercent { get; set; }

        int LpMarkupPips { get; set; }

        string LwId { get; set; }
    }
}
