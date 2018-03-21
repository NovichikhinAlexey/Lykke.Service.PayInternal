using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core.Domain.Asset;

namespace Lykke.Service.PayInternal.AzureRepositories.Asset
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class AssetAvailabilityByMerchantEntity : AzureTableEntity, IAssetAvailabilityByMerchant
    {
        public static AssetAvailabilityByMerchantEntity Create(IAssetAvailabilityByMerchant src)
        {
            return new AssetAvailabilityByMerchantEntity
            {
                PartitionKey = GeneratePartitionKey(src.MerchantId),
                RowKey = GenerateRowKey(src.MerchantId),
                MerchantId = src.MerchantId,
                PaymentAssets = src.PaymentAssets,
                SettlementAssets = src.SettlementAssets
            };
        }

        public static string GeneratePartitionKey(string merchantId)
        {
            return merchantId;
        }

        public static string GenerateRowKey(string merchantId)
        {
            return merchantId;
        }

        public string MerchantId { get; set; }

        public string PaymentAssets { get; set; }

        public string SettlementAssets { get; set; }
    }
}
