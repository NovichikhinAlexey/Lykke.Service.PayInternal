namespace Lykke.Service.PayInternal.Client.Models.Asset
{
    public class AssetMerchantSettingsResponse
    {
        public string MerchantId { get; set; }
        public string PaymentAssets { get; set; }
        public string SettlementAssets { get; set; }
    }
}
