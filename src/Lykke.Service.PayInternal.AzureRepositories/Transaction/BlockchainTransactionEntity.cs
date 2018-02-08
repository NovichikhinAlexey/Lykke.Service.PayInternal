﻿using System;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.AzureRepositories.Transaction
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class BlockchainTransactionEntity : AzureTableEntity, IBlockchainTransaction
    {
        private decimal _amount;
        private int _confirmations;
        private DateTime _firstSeen;

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

        public DateTime FirstSeen
        {
            get => _firstSeen;
            set
            {
                _firstSeen = value;
                MarkValueTypePropertyAsDirty(nameof(FirstSeen));
            }
        }

        public string AssetId { get; set; }

        public string Blockchain { get; set; }

        internal void Map(IBlockchainTransaction blockchainTransaction)
        {
            TransactionId = blockchainTransaction.TransactionId;
            PaymentRequestId = blockchainTransaction.PaymentRequestId;
            Amount = blockchainTransaction.Amount;
            BlockId = blockchainTransaction.BlockId;
            Confirmations = blockchainTransaction.Confirmations;
            WalletAddress = blockchainTransaction.WalletAddress;
            FirstSeen = blockchainTransaction.FirstSeen;
            AssetId = blockchainTransaction.AssetId;
            Blockchain = blockchainTransaction.Blockchain;
        }
    }
}