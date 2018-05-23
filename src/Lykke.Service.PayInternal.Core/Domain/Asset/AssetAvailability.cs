namespace Lykke.Service.PayInternal.Core.Domain.Asset
{
    public class AssetAvailability : IAssetAvailability
    {
        public string AssetId { get; set; }
        public BlockchainType Network { get; set; }
        public bool PaymentAvailable { get; set; }
        public bool SettlementAvailable { get; set; }
    }
}
