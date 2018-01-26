using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.PayInternal.AzureRepositories.Merchant
{
    public class MerchantEntity : TableEntity, IMerchant
    {
        public MerchantEntity()
        {
        }

        public MerchantEntity(string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        {
        }

        public string Id => RowKey;
        
        public string Name { get; set; }

        public string PublicKey { get; set; }

        public string ApiKey { get; set; }

        public double DeltaSpread { get; set; }

        public int TimeCacheRates { get; set; }

        public double LpMarkupPercent { get; set; }

        public int LpMarkupPips { get; set; }
        
        public double MarkupFixedFee { get; set; }

        public string LwId { get; set; }

        internal void Map(IMerchant merchant)
        {
            ApiKey = merchant.ApiKey;
            DeltaSpread = merchant.DeltaSpread;
            LpMarkupPercent = merchant.LpMarkupPercent;
            LpMarkupPips = merchant.LpMarkupPips;
            MarkupFixedFee = merchant.MarkupFixedFee;
            LwId = merchant.LwId;
            Name = merchant.Name;
            PublicKey = merchant.PublicKey;
            TimeCacheRates = merchant.TimeCacheRates;
        }
    }
}
