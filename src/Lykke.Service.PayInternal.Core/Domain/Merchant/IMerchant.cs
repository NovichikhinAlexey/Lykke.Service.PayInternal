namespace Lykke.Service.PayInternal.Core.Domain.Merchant
{
    public interface IMerchant
    {
        string Id { get; }

        string Name { get; set; }

        string DisplayName { get; set; }

        string PublicKey { get; set; }

        string ApiKey { get; set; }

        int TimeCacheRates { get; set; }
        
        string LwId { get; set; }
    }
}
