using System;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Asset;

namespace Lykke.Service.PayInternal.AzureRepositories.Asset
{
    public class AssetMerchantSettingsRepository : IAssetMerchantSettingsRepository
    {
        private readonly INoSQLTableStorage<AssetMerchantSettingsEntity> _tableStorage;

        public AssetMerchantSettingsRepository(INoSQLTableStorage<AssetMerchantSettingsEntity> tableStorage)
        {
            _tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
        }

        public async Task<IAssetMerchantSettings> GetAsync(string merchantId)
        {
            AssetMerchantSettingsEntity settingsEntity = await _tableStorage.GetDataAsync(
                AssetMerchantSettingsEntity.GeneratePartitionKey(merchantId),
                AssetMerchantSettingsEntity.GenerateRowKey(merchantId));

            return settingsEntity;
        }

        public async Task<IAssetMerchantSettings> SetAsync(string paymentAssets, string settlementAssets, string merchantId)
        {
            string partitionKey = AssetMerchantSettingsEntity.GeneratePartitionKey(merchantId);
            string rowKey = AssetMerchantSettingsEntity.GenerateRowKey(merchantId);

            AssetMerchantSettingsEntity exItem = await _tableStorage.GetDataAsync(partitionKey, rowKey);
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
            var newItem = AssetMerchantSettingsEntity.Create(new AssetMerchantSettings
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
