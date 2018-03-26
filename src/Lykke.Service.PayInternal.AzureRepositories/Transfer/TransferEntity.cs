using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.AzureRepositories.Serializers;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.AzureRepositories.Transfer
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class TransferEntity : AzureTableEntity
    {
        private DateTime _createdOn;

        public string Id => RowKey;

        public string AssetId { get; set; }

        public string Blockchain { get; set; }

        [ValueSerializer(typeof(AmountsListSerializer))]
        public IEnumerable<TransferAmount> Amounts { get; set; }

        [ValueSerializer(typeof(TransactionListSerializer))]
        public IEnumerable<TransferTransaction> Transactions { get; set; }

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
                return Guid.NewGuid().ToString();
            }

            public static TransferEntity Create(ITransfer src)
            {
                return new TransferEntity
                {
                    PartitionKey = GeneratePartitionKey(src.CreatedOn),
                    RowKey = GenerateRowKey(),
                    AssetId = src.AssetId,
                    Blockchain = src.Blockchain,
                    CreatedOn = src.CreatedOn,
                    Amounts = src.Amounts.ToList(),
                    Transactions = src.Transactions
                };
            }
        }
    }
}
