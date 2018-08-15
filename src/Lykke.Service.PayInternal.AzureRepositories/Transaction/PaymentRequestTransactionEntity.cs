using System;
using AutoMapper;
using AzureStorage.Tables.Templates.Index;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.AzureRepositories.Serializers;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.AzureRepositories.Transaction
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class PaymentRequestTransactionEntity : AzureTableEntity
    {
        private decimal _amount;
        private int _confirmations;
        private DateTime? _firstSeen;
        private DateTime? _dueDate;
        private TransactionType _transactionType;
        private BlockchainType _blockchain;
        private TransactionIdentityType _transactionIdentityType;

        public PaymentRequestTransactionEntity()
        {
        }

        public PaymentRequestTransactionEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string Id => RowKey;
        
        public string TransactionId { get; set; }

        public string TransferId { get; set; }
        
        public string PaymentRequestId { get; set; }

        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                MarkValueTypePropertyAsDirty(nameof(Amount));
            }
        }
        
        public string BlockId { get; set; }
        
        public int Confirmations
        {
            get => _confirmations;
            set
            {
                _confirmations = value;
                MarkValueTypePropertyAsDirty(nameof(Confirmations));
            }
        }
        
        public string WalletAddress { get; set; }

        [ValueSerializer(typeof(StringListSerializer))]
        public string[] SourceWalletAddresses { get; set; }
     
        public DateTime? FirstSeen
        {
            get => _firstSeen;
            set
            {
                _firstSeen = value;
                MarkValueTypePropertyAsDirty(nameof(FirstSeen));
            }
        }

        public string AssetId { get; set; }

        public BlockchainType Blockchain
        {
            get => _blockchain;
            set
            {
                _blockchain = value;
                MarkValueTypePropertyAsDirty(nameof(Blockchain));
            }
        }

        public TransactionType TransactionType
        {
            get => _transactionType;
            set
            {
                _transactionType = value;
                MarkValueTypePropertyAsDirty(nameof(TransactionType));
            }
        }

        public string ContextData { get; set; }

        public DateTime? DueDate
        {
            get => _dueDate;
            set
            {
                _dueDate = value;
                MarkValueTypePropertyAsDirty(nameof(DueDate));
            }
        }

        public TransactionIdentityType IdentityType
        {
            get => _transactionIdentityType;
            set
            {
                _transactionIdentityType = value;
                MarkValueTypePropertyAsDirty(nameof(IdentityType));
            }
        }

        public string Identity { get; set; }

        public static class ByWalletAddress
        {
            public static string GeneratePartitionKey(string walletAddress)
            {
                return walletAddress ?? "NoWalletAddress";
            }

            public static string GenerateRowKey(BlockchainType blockchain, TransactionIdentityType identityType, string identity)
            {
                return $"{blockchain.ToString()}_{identityType.ToString()}_{identity}";
            }

            public static PaymentRequestTransactionEntity Create(IPaymentRequestTransaction src)
            {
                var entity = new PaymentRequestTransactionEntity
                {
                    PartitionKey = GeneratePartitionKey(src.WalletAddress),
                    RowKey = GenerateRowKey(src.Blockchain, src.IdentityType, src.Identity)
                };

                return Mapper.Map(src, entity);
            }
        }

        public static class IndexByIdentity
        {
            public static string GeneratePartitionKey(BlockchainType blockchain, TransactionIdentityType identityType, string identity)
            {
                return $"{blockchain.ToString()}_{identityType.ToString()}_{identity}";
            }

            public static string GenerateRowKey(string walletAddress)
            {
                return walletAddress ?? "NoWalletAddress";
            }

            public static AzureIndex Create(PaymentRequestTransactionEntity entity)
            {
                return AzureIndex.Create(
                    GeneratePartitionKey(entity.Blockchain, entity.IdentityType, entity.Identity),
                    GenerateRowKey(entity.WalletAddress),
                    entity);
            }
        }

        public static class IndexByDueDate
        {
            public static string GeneratePartitionKey(DateTime dueDate)
            {
                var dueDateIso = dueDate.ToString("O");

                return $"DD_{dueDateIso}";
            }

            public static string GenerateRowKey(TransactionIdentityType identityType, string identity, BlockchainType blockchain, string walletAddress)
            {
                return $"{blockchain.ToString()}_{identityType.ToString()}_{identity}_{walletAddress}";
            }

            public static AzureIndex Create(PaymentRequestTransactionEntity entity)
            {
                return entity.DueDate.HasValue
                    ? AzureIndex.Create(
                        GeneratePartitionKey(entity.DueDate.Value),
                        GenerateRowKey(entity.IdentityType, entity.Identity, entity.Blockchain, entity.WalletAddress),
                        entity)
                    : null;
            }
        }
    }
}
