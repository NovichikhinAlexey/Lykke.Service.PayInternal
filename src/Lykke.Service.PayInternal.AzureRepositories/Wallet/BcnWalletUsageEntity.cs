using System;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Wallet;

namespace Lykke.Service.PayInternal.AzureRepositories.Wallet
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class BcnWalletUsageEntity : AzureTableEntity
    {
        #region private fields
        private BlockchainType _blockchain;
        private DateTime _since;
        #endregion

        public string WalletAddress { get; set; }

        public BlockchainType Blockchain
        {
            get => _blockchain;

            set
            {
                _blockchain = value;
                MarkValueTypePropertyAsDirty(nameof(Blockchain));
            }
        }

        public string OccupiedBy { get; set; }

        public DateTime Since
        {
            get => _since;

            set
            {
                _since = value;
                MarkValueTypePropertyAsDirty(nameof(Since));
            }
        }

        public static class ByWalletAddress
        {
            public static string GeneratePartitionKey(string walletAddress)
            {
                return walletAddress;
            }

            public static string GenerateRowKey(BlockchainType blockchain)
            {
                return blockchain.ToString();
            }

            public static BcnWalletUsageEntity Create(IBcnWalletUsage src)
            {
                return new BcnWalletUsageEntity
                {
                    PartitionKey = GeneratePartitionKey(src.WalletAddress),
                    RowKey = GenerateRowKey(src.Blockchain),
                    Blockchain = src.Blockchain,
                    WalletAddress = src.WalletAddress,
                    OccupiedBy = src.OccupiedBy,
                    Since = src.Since
                };
            }
        }
    }
}
