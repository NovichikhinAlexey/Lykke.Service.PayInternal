using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Asset;

namespace Lykke.Service.PayInternal.AzureRepositories.Asset
{
    public class AssetAvailabilityRepository : IAssetAvailabilityRepository
    {
        private readonly INoSQLTableStorage<AssetAvailabilityEntity> _tableStorage;

        public AssetAvailabilityRepository(INoSQLTableStorage<AssetAvailabilityEntity> tableStorage)
        {
            _tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
        }

        public async Task<IReadOnlyList<IAssetAvailability>> GetAsync(AssetAvailabilityType availability)
        {
            IList<AssetAvailabilityEntity> result = await _tableStorage.GetDataAsync(a =>
            {
                switch (availability)
                {
                    case AssetAvailabilityType.Payment:
                        return a.PaymentAvailable;
                    case AssetAvailabilityType.Settlement:
                        return a.SettlementAvailable;
                    default:
                        throw new Exception($"Unexpected asset availability type {availability.ToString()}");
                }
            });

            return result.ToList();
        }

        public async Task<IAssetAvailability> SetAsync(string assetId, AssetAvailabilityType availability, bool value)
        {
            string partitionKey = AssetAvailabilityEntity.ByAsset.GeneratePartitionKey(assetId);
            string rowKey = AssetAvailabilityEntity.ByAsset.GenerateRowKey();

            AssetAvailabilityEntity exItem = await _tableStorage.GetDataAsync(partitionKey, rowKey);

            if (exItem != null)
            {
                AssetAvailabilityEntity merged = await _tableStorage.MergeAsync(partitionKey, rowKey, item =>
                {
                    switch (availability)
                    {
                        case AssetAvailabilityType.Payment:
                            item.PaymentAvailable = value;
                            break;
                        case AssetAvailabilityType.Settlement:
                            item.SettlementAvailable = value;
                            break;
                        default:
                            throw new Exception($"Unexpected asset availability type {availability.ToString()}");
                    }

                    return item;
                });

                return merged;
            }

            var newItem = AssetAvailabilityEntity.ByAsset.Create(new AssetAvailability
            {
                AssetId = assetId,
                PaymentAvailable = availability == AssetAvailabilityType.Payment && value,
                SettlementAvailable = availability == AssetAvailabilityType.Settlement && value
            });

            await _tableStorage.InsertAsync(newItem);

            return newItem;
        }
    }
}
