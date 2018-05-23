using AutoMapper;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Asset;

namespace Lykke.Service.PayInternal.AzureRepositories.Asset
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class AssetAvailabilityEntity : AzureTableEntity
    {
        private bool _paymentAvailable;
        private bool _settlementAvailable;
        private BlockchainType _network;

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

        public BlockchainType Network
        {
            get => _network;

            set
            {
                _network = value;
                MarkValueTypePropertyAsDirty(nameof(Network));
            }
        }

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
                var entity = new AssetAvailabilityEntity
                {
                    PartitionKey = GeneratePartitionKey(src.AssetId),
                    RowKey = GenerateRowKey(src.AssetId),
                };

                return Mapper.Map(src, entity);
            }
        }
    }
}
