using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Asset;

namespace Lykke.Service.PayInternal.AzureRepositories.Asset
{
    public class AssetGeneralAvailabilityRepository : IAssetGeneralAvailabilityRepository
    {
        private readonly INoSQLTableStorage<AssetAvailabilityEntity> _tableStorage;

        public AssetGeneralAvailabilityRepository([NotNull] INoSQLTableStorage<AssetAvailabilityEntity> tableStorage)
        {
            _tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
        }

        public async Task<IReadOnlyList<IAssetAvailability>> GetByTypeAsync(AssetAvailabilityType availabilityType)
        {
            IList<AssetAvailabilityEntity> result = await _tableStorage.GetDataAsync(a =>
            {
                switch (availabilityType)
                {
                    case AssetAvailabilityType.Payment:
                        return a.PaymentAvailable;
                    case AssetAvailabilityType.Settlement:
                        return a.SettlementAvailable;
                    default:
                        throw new Exception($"Unexpected asset availability type {availabilityType.ToString()}");
                }
            });

            return Mapper.Map<IList<AssetAvailability>>(result).ToList();
        }

        public async Task<IReadOnlyList<IAssetAvailability>> GetAsync()
        {
            IList<AssetAvailabilityEntity> result = await _tableStorage.GetDataAsync();

            return Mapper.Map<IList<AssetAvailability>>(result).ToList();
        }

        public async Task<IAssetAvailability> SetAsync(IAssetAvailability availability)
        {
            string partitionKey = AssetAvailabilityEntity.ByAsset.GeneratePartitionKey(availability.AssetId);
            string rowKey = AssetAvailabilityEntity.ByAsset.GenerateRowKey(availability.AssetId);

            AssetAvailabilityEntity exItem = await _tableStorage.GetDataAsync(partitionKey, rowKey);

            if (exItem != null)
            {
                AssetAvailabilityEntity merged = await _tableStorage.MergeAsync(partitionKey, rowKey, item =>
                {
                    item.PaymentAvailable = availability.PaymentAvailable;
                    item.SettlementAvailable = availability.SettlementAvailable;
                    item.Network = availability.Network;

                    return item;
                });

                return Mapper.Map<AssetAvailability>(merged);
            }

            var newItem = AssetAvailabilityEntity.ByAsset.Create(availability);

            await _tableStorage.InsertAsync(newItem);

            return Mapper.Map<AssetAvailability>(newItem);
        }
    }
}
