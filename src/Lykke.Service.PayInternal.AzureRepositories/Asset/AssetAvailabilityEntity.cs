using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core.Domain.Asset;

namespace Lykke.Service.PayInternal.AzureRepositories.Asset
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class AssetAvailabilityEntity : AzureTableEntity, IAssetAvailability
    {
        private bool _paymentAvailable;
        private bool _settlementAvailable;

        public static class ByAsset
        {
            public static string GeneratePartitionKey(string assetId)
            {
                return assetId;
            }

            public static string GenerateRowKey(string assetId)
            {
                return assetId;
            }

            public static AssetAvailabilityEntity Create(IAssetAvailability src)
            {
                return new AssetAvailabilityEntity
                {
                    PartitionKey = GeneratePartitionKey(src.AssetId),
                    RowKey = GenerateRowKey(src.AssetId),
                    AssetId = src.AssetId,
                    PaymentAvailable = src.PaymentAvailable,
                    SettlementAvailable = src.SettlementAvailable
                };
            }
        }

        public string AssetId { get; set; }

        public bool PaymentAvailable
        {
            get => _paymentAvailable;

            set
            {
                _paymentAvailable = value;
                MarkValueTypePropertyAsDirty(nameof(PaymentAvailable));
            }
        }

        public bool SettlementAvailable
        {
            get => _settlementAvailable;

            set
            {
                _settlementAvailable = value;
                MarkValueTypePropertyAsDirty(nameof(SettlementAvailable));
            }
        }
    }
}
