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
    public class AssetGeneralSettingsRepository : IAssetGeneralSettingsRepository
    {
        private readonly INoSQLTableStorage<AssetGeneralSettingsEntity> _tableStorage;

        public AssetGeneralSettingsRepository([NotNull] INoSQLTableStorage<AssetGeneralSettingsEntity> tableStorage)
        {
            _tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
        }

        public async Task<IReadOnlyList<IAssetGeneralSettings>> GetAsync(AssetAvailabilityType availabilityType)
        {
            IList<AssetGeneralSettingsEntity> result = await _tableStorage.GetDataAsync(a =>
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

            return Mapper.Map<IList<AssetGeneralSettings>>(result).ToList();
        }

        public async Task<IReadOnlyList<IAssetGeneralSettings>> GetAsync()
        {
            IList<AssetGeneralSettingsEntity> result = await _tableStorage.GetDataAsync();

            return Mapper.Map<IList<AssetGeneralSettings>>(result).ToList();
        }

        public async Task<IAssetGeneralSettings> GetAsync(string assetId)
        {
            AssetGeneralSettingsEntity entity = await _tableStorage.GetDataAsync(
                AssetGeneralSettingsEntity.ByAsset.GeneratePartitionKey(assetId),
                AssetGeneralSettingsEntity.ByAsset.GenerateRowKey(assetId));

            return Mapper.Map<AssetGeneralSettings>(entity);
        }

        public async Task<IAssetGeneralSettings> SetAsync(IAssetGeneralSettings availability)
        {
            string partitionKey = AssetGeneralSettingsEntity.ByAsset.GeneratePartitionKey(availability.AssetId);
            string rowKey = AssetGeneralSettingsEntity.ByAsset.GenerateRowKey(availability.AssetId);

            AssetGeneralSettingsEntity exItem = await _tableStorage.GetDataAsync(partitionKey, rowKey);

            if (exItem != null)
            {
                AssetGeneralSettingsEntity merged = await _tableStorage.MergeAsync(partitionKey, rowKey, item =>
                {
                    item.PaymentAvailable = availability.PaymentAvailable;
                    item.SettlementAvailable = availability.SettlementAvailable;
                    item.Network = availability.Network;

                    return item;
                });

                return Mapper.Map<AssetGeneralSettings>(merged);
            }

            var newItem = AssetGeneralSettingsEntity.ByAsset.Create(availability);

            await _tableStorage.InsertAsync(newItem);

            return Mapper.Map<AssetGeneralSettings>(newItem);
        }
    }
}
