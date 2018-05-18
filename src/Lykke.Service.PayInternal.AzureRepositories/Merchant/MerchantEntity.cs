using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core.Domain.Merchant;

namespace Lykke.Service.PayInternal.AzureRepositories.Merchant
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class MerchantEntity : AzureTableEntity, IMerchant
    {
        private int _timeCacheRates;

        public MerchantEntity()
        {
        }

        public MerchantEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string Id => RowKey;
        
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string PublicKey { get; set; }

        public string ApiKey { get; set; }

        public int TimeCacheRates
        {
            get => _timeCacheRates;
            set
            {
                _timeCacheRates = value;
                MarkValueTypePropertyAsDirty(nameof(TimeCacheRates));
            }
        }
        
        public string LwId { get; set; }
    }
}
