namespace Lykke.Service.PayInternal.Core.Domain.Asset
{
    public interface IAssetGeneralSettings
    {
        string AssetId { get; set; }

        BlockchainType Network { get; set; }

        bool PaymentAvailable { get; set; }

        bool SettlementAvailable { get; set; }
    }
}
