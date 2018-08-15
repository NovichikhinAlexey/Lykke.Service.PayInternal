using System;
using AutoMapper;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core.Domain.AssetPair;

namespace Lykke.Service.PayInternal.AzureRepositories.AssetPair
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class AssetPairRateEntity : AzureTableEntity
    {
        private decimal _bidPrice;
        private decimal _askPrice;
        private DateTime _createdOn;

        public string Id => RowKey;

        public string BaseAssetId { get; set; }

        public string QuotingAssetId { get; set; }

        public decimal BidPrice
        {
            get => _bidPrice;
            set
            {
                _bidPrice = value;
                MarkValueTypePropertyAsDirty(nameof(BidPrice));
            }
        }

        public decimal AskPrice
        {
            get => _askPrice;
            set
            {
                _askPrice = value;
                MarkValueTypePropertyAsDirty(nameof(AskPrice));
            }
        }

        public DateTime CreatedOn
        {
            get => _createdOn;
            set
            {
                _createdOn = value;
                MarkValueTypePropertyAsDirty(nameof(CreatedOn));
            }
        }

        public static class ByDate
        {
            public static string GeneratePartitionKey(DateTime createdOn)
            {
                return createdOn.ToString("yyyy-MM-dd");
            }

            public static string GenerateRowKey()
            {
                return Guid.NewGuid().ToString("D");
            }

            public static AssetPairRateEntity Create(IAssetPairRate src)
            {
                var entity = new AssetPairRateEntity
                {
                    PartitionKey = GeneratePartitionKey(src.CreatedOn),
                    RowKey = GenerateRowKey()
                };

                return Mapper.Map(src, entity);
            }
        }
    }
}
