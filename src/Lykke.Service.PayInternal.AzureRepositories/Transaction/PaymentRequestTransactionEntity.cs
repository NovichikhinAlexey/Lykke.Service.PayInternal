using System;
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
        private DateTime _dueDate;
        private TransactionType _transactionType;
        private BlockchainType _blockchain;

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

        public DateTime DueDate
        {
            get => _dueDate;
            set
            {
                _dueDate = value;
                MarkValueTypePropertyAsDirty(nameof(DueDate));
            }
        }
    }
}
