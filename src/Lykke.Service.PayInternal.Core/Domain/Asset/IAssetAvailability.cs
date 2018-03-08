namespace Lykke.Service.PayInternal.Core.Domain.Asset
{
    public interface IAssetAvailability
    {
        string AssetId { get; set; }
        bool PaymentAvailable { get; set; }
        bool SettlementAvailable { get; set; }
    }
}
