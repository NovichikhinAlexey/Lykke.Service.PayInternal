using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Asset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.AzureRepositories.Asset
{
    public class AssetAvailabilityByMerchantRepository : IAssetAvailabilityByMerchantRepository
    {
        private readonly INoSQLTableStorage<AssetAvailabilityByMerchantEntity> _tableStorage;
        public AssetAvailabilityByMerchantRepository(INoSQLTableStorage<AssetAvailabilityByMerchantEntity> tableStorage)
        {
            _tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
        }

        public async Task<IAssetAvailabilityByMerchant> GetAsync(string merchantId)
        {
            var result = await _tableStorage.GetDataAsync(a => a.MerchantId == merchantId);
            return result.FirstOrDefault();
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
                exItem.AssetsPayment = paymentAssets;
                exItem.AssetsSettlement = settlementAssets;
                await _tableStorage.InsertOrMergeAsync(exItem);
                return exItem;
            }
            var newItem = AssetAvailabilityByMerchantEntity.Create(new AssetAvailabilityByMerchant
            {
                MerchantId = merchantId,
                AssetsPayment = paymentAssets,
                AssetsSettlement = settlementAssets
            });

            await _tableStorage.InsertAsync(newItem);
            return newItem;
        }
    }
}
