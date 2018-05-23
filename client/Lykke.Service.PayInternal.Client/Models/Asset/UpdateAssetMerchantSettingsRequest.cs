namespace Lykke.Service.PayInternal.Client.Models.Asset
{
    public class UpdateAssetMerchantSettingsRequest
    {
        public string PaymentAssets { get; set; }
        public string SettlementAssets { get; set; }
        public string MerchantId { get; set; }
    }
}
