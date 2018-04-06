using System;
using System.Collections.Generic;
using AzureStorage.Tables.Templates.Index;
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
        public IEnumerable<BlockchainWallet> BlockchainWallets { get; set; }

        public static class ByMerchantId
        {
            public static string GeneratePartitionKey(string merchantId)
            {
                return merchantId;
            }

            public static string GenerateRowKey(string walletId = null)
            {
                return walletId ?? Guid.NewGuid().ToString();
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
                    BlockchainWallets = src.BlockchainWallets
                };
            }
        }

        public static class IndexByWalletId
        {
            public static string GeneratePartitionKey(string walletId)
            {
                return walletId;
            }

            public static string GenerateRowKey()
            {
                return "WalletIdIndex";
            }

            public static AzureIndex Create(VirtualWalletEntity src)
            {
                return AzureIndex.Create(GeneratePartitionKey(src.Id), GenerateRowKey(), src);
            }
        }

        public static class IndexByDueDate
        {
            public static string GeneratePartitionKey(DateTime dueDate)
            {
                var dueDateIso = dueDate.ToString("O");

                return $"DD_{dueDateIso}";
            }

            public static string GenerateRowKey(string walletId)
            {
                return walletId;
            }

            public static AzureIndex Create(VirtualWalletEntity src)
            {
                return AzureIndex.Create(GeneratePartitionKey(src.DueDate), GenerateRowKey(src.Id), src);
            }
        }
    }
}
