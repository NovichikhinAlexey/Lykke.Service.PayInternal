using System;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.AzureRepositories.Serializers;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.AzureRepositories.Transaction
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class BlockchainTransactionEntity : AzureTableEntity, IBlockchainTransaction
    {
        private decimal _amount;
        private int _confirmations;
        private DateTime? _firstSeen;
        private DateTime _dueDate;
        private TransactionType _transactionType;
        

        public BlockchainTransactionEntity()
        {
        }

        public BlockchainTransactionEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string Id => RowKey;

        public string TransactionId { get; set; }

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

        [CanBeNull] public string Blockchain { get; set; }

        public TransactionType TransactionType
        {
            get => _transactionType;
            set
            {
                _transactionType = value;
                MarkValueTypePropertyAsDirty(nameof(TransactionType));
            }
        }

        public DateTime DueDate
        {
            get => _dueDate;
            set
            {
                _dueDate = value;
                MarkValueTypePropertyAsDirty(nameof(DueDate));
            }
        }

        internal void Map(IBlockchainTransaction blockchainTransaction)
        {
            TransactionId = blockchainTransaction.TransactionId;
            PaymentRequestId = blockchainTransaction.PaymentRequestId;
            Amount = blockchainTransaction.Amount;
            BlockId = blockchainTransaction.BlockId;
            Confirmations = blockchainTransaction.Confirmations;
            WalletAddress = blockchainTransaction.WalletAddress;
            SourceWalletAddresses = blockchainTransaction.SourceWalletAddresses;
            FirstSeen = blockchainTransaction.FirstSeen;
            AssetId = blockchainTransaction.AssetId;
            Blockchain = blockchainTransaction.Blockchain;
            TransactionType = blockchainTransaction.TransactionType;
            DueDate = blockchainTransaction.DueDate;
        }
    }
}
