using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core.Domain.Merchant;

namespace Lykke.Service.PayInternal.AzureRepositories.Merchant
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class MerchantEntity : AzureTableEntity, IMerchant
    {
        private double _deltaSpread;
        private int _timeCacheRates;
        private double _lpMarkupPercent;
        private int _lpMarkupPips;
        private double _markupFixedFee;

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

        public double DeltaSpread
        {
            get => _deltaSpread;
            set
            {
                _deltaSpread = value;
                MarkValueTypePropertyAsDirty(nameof(DeltaSpread));
            }
        }
        
        public int TimeCacheRates
        {
            get => _timeCacheRates;
            set
            {
                _timeCacheRates = value;
                MarkValueTypePropertyAsDirty(nameof(TimeCacheRates));
            }
        }
        
        public double LpMarkupPercent
        {
            get => _lpMarkupPercent;
            set
            {
                _lpMarkupPercent = value;
                MarkValueTypePropertyAsDirty(nameof(LpMarkupPercent));
            }
        }
        
        public int LpMarkupPips
        {
            get => _lpMarkupPips;
            set
            {
                _lpMarkupPips = value;
                MarkValueTypePropertyAsDirty(nameof(LpMarkupPips));
            }
        }
        
        public double MarkupFixedFee
        {
            get => _markupFixedFee;
            set
            {
                _markupFixedFee = value;
                MarkValueTypePropertyAsDirty(nameof(MarkupFixedFee));
            }
        }
        
        public string LwId { get; set; }
    }
}
