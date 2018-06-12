using System;
using System.Collections.Generic;
using AutoMapper;
using AzureStorage.Tables.Templates.Index;
using Common;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.MerchantWallet;
using Lykke.Service.PayInternal.Core.Exceptions;

namespace Lykke.Service.PayInternal.AzureRepositories.MerchantWallet
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class MerchantWalletEntity : AzureTableEntity
    {
        private BlockchainType _network;
        private DateTime _createdOn;

        public string Id { get; set; }

        public string MerchantId { get; set; }

        public BlockchainType Network
        {
            get => _network;
            set
            {
                _network = value;
                MarkValueTypePropertyAsDirty(nameof(Network));
            }
        }

        public string WalletAddress { get; set; }

        public string DisplayName { get; set; }

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
        public IList<string> IncomingPaymentDefaults { get; set; }

        [JsonValueSerializer]
        public IList<string> OutgoingPaymentDefaults { get; set; }

        public static class ByMerchant
        {
            public static string GeneratePartitionKey(string merchantId)
            {
                if (!merchantId.IsValidPartitionOrRowKey())
                    throw new InvalidRowKeyValueException(nameof(merchantId), merchantId);

                return merchantId;
            }

            public static string GenerateRowKey(BlockchainType network, string walletAddress)
            {
                if (!walletAddress.IsValidPartitionOrRowKey())
                    throw new InvalidRowKeyValueException(nameof(walletAddress), walletAddress);

                return $"{network.ToString()}_{walletAddress}";
            }

            public static MerchantWalletEntity Create(IMerchantWallet src)
            {
                var entity = new MerchantWalletEntity
                {
                    PartitionKey = GeneratePartitionKey(src.MerchantId),
                    RowKey = GenerateRowKey(src.Network, src.WalletAddress),
                    Id = Guid.NewGuid().ToString("D")
                };

                return Mapper.Map(src, entity);
            }
        }

        public static class IndexById
        {
            public static string GeneratePartitionKey(string id)
            {
                if (!id.IsValidPartitionOrRowKey())
                    throw new InvalidRowKeyValueException(nameof(id), id);

                return id;
            }

            public static string GenerateRowKey()
            {
                return "IndexById";
            }

            public static AzureIndex Create(MerchantWalletEntity src)
            {
                return AzureIndex.Create(GeneratePartitionKey(src.Id), GenerateRowKey(), src);
            }
        }
    }
}
