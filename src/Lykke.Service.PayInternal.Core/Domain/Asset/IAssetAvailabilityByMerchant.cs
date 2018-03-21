namespace Lykke.Service.PayInternal.Core.Domain.Asset
{
    public interface IAssetAvailabilityByMerchant
    {
        string MerchantId { get; set; }

        string PaymentAssets { get; set; }

        string SettlementAssets { get; set; }
    }
}
