using System;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Asset;

namespace Lykke.Service.PayInternal.AzureRepositories.Asset
{
    public class AssetPersonalAvailabilityRepository : IAssetPersonalAvailabilityRepository
    {
        private readonly INoSQLTableStorage<AssetAvailabilityByMerchantEntity> _tableStorage;

        public AssetPersonalAvailabilityRepository(INoSQLTableStorage<AssetAvailabilityByMerchantEntity> tableStorage)
        {
            _tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
        }

        public async Task<IAssetAvailabilityByMerchant> GetAsync(string merchantId)
        {
            AssetAvailabilityByMerchantEntity entity = await _tableStorage.GetDataAsync(
                AssetAvailabilityByMerchantEntity.GeneratePartitionKey(merchantId),
                AssetAvailabilityByMerchantEntity.GenerateRowKey());

            return entity;
        }

        public async Task<IAssetAvailabilityByMerchant> SetAsync(string paymentAssets, string settlementAssets, string merchantId)
        {
            string partitionKey = AssetAvailabilityByMerchantEntity.GeneratePartitionKey(merchantId);
            string rowKey = AssetAvailabilityByMerchantEntity.GenerateRowKey();

            AssetAvailabilityByMerchantEntity exItem = await _tableStorage.GetDataAsync(partitionKey, rowKey);
            if (exItem != null && string.IsNullOrEmpty(settlementAssets) && string.IsNullOrEmpty(paymentAssets))
            {
                await _tableStorage.DeleteAsync(exItem);
                return null;
            }
            if (exItem != null)
            {
                exItem.PaymentAssets = paymentAssets;
                exItem.SettlementAssets = settlementAssets;
                await _tableStorage.InsertOrMergeAsync(exItem);
                return exItem;
            }
            var newItem = AssetAvailabilityByMerchantEntity.Create(new AssetAvailabilityByMerchant
            {
                MerchantId = merchantId,
                PaymentAssets = paymentAssets,
                SettlementAssets = settlementAssets
            });

            await _tableStorage.InsertAsync(newItem);
            return newItem;
        }
    }
}
