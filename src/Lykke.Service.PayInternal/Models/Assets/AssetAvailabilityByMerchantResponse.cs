namespace Lykke.Service.PayInternal.Models.Assets
{
    public class AssetAvailabilityByMerchantResponse
    {
        public string MerchantId { get; set; }

        public string PaymentAssets { get; set; }

        public string SettlementAssets { get; set; }
    }
}
