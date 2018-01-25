using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.PayInternal.AzureRepositories.Merchant
{
    public class MerchantEntity : TableEntity, IMerchant
    {
        public static string GeneratePartitionKey()
        {
            return "M";
        }

        public static string GenerateRowKey(string merchantId)
        {
            return merchantId;
        }

        public static MerchantEntity Create(IMerchant src)
        {
            return new MerchantEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(src.Id),
                ApiKey = src.ApiKey,
                DeltaSpread = src.DeltaSpread,
                LpMarkupPercent = src.LpMarkupPercent,
                LpMarkupPips = src.LpMarkupPips,
                LwId = src.LwId,
                Name = src.Name,
                PublicKey = src.PublicKey,
                TimeCacheRates = src.TimeCacheRates
            };
        }

        public string Id => RowKey;

        public string Name { get; set; }

        public string PublicKey { get; set; }

        public string ApiKey { get; set; }

        public double DeltaSpread { get; set; }

        public int TimeCacheRates { get; set; }

        public double LpMarkupPercent { get; set; }

        public int LpMarkupPips { get; set; }

        public string LwId { get; set; }
    }
}
