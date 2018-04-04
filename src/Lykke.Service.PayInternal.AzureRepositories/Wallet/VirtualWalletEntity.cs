using System;
using System.Collections.Generic;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core.Domain.Wallet;

namespace Lykke.Service.PayInternal.AzureRepositories.Wallet
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class VirtualWalletEntity : AzureTableEntity
    {
        #region private fields
        private DateTime _dueDate;

        private DateTime _createdOn;
        #endregion

        public string Id => RowKey;

        public string MerchantId { get; set; }

        public DateTime DueDate
        {
            get => _dueDate;

            set
            {
                _dueDate = value;
                MarkValueTypePropertyAsDirty(nameof(DueDate));
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

        [JsonValueSerializer]
        public IEnumerable<OriginalWallet> OriginalWallets { get; set; }

        public static class ByMerchantId
        {
            public static string GeneratePartitionKey(string merchantId)
            {
                return merchantId;
            }

            public static string GenerateRowKey()
            {
                return Guid.NewGuid().ToString();
            }

            public static VirtualWalletEntity Create(IVirtualWallet src)
            {
                return new VirtualWalletEntity
                {
                    PartitionKey = GeneratePartitionKey(src.MerchantId),
                    RowKey = GenerateRowKey(),
                    DueDate = src.DueDate,
                    MerchantId = src.MerchantId,
                    CreatedOn = src.CreatedOn,
                    OriginalWallets = src.OriginalWallets
                };
            }
        }
    }
}
