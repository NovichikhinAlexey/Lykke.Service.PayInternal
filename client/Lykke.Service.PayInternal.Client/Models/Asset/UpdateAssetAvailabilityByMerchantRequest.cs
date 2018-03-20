namespace Lykke.Service.PayInternal.Client.Models.Asset
{
    public class UpdateAssetAvailabilityByMerchantRequest
    {
        public string PaymentAssets { get; set; }
        public string SettlementAssets { get; set; }
        public string MerchantId { get; set; }
    }
}
