using AzureStorage.Tables.Templates.Index;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using System;
using AutoMapper;
using Common;
using Lykke.Service.PayInternal.Core.Domain.Groups;
using Lykke.Service.PayInternal.Core.Exceptions;

namespace Lykke.Service.PayInternal.AzureRepositories.MerchantGroup
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class MerchantGroupEntity : AzureTableEntity
    {
        private MerchantGroupUse _merchantGroupUse;

        public string Id => RowKey;

        public string DisplayName { get; set; }

        public string OwnerId { get; set; }

        public string Merchants { get; set; }

        public MerchantGroupUse MerchantGroupUse
        {
            get => _merchantGroupUse;
            set
            {
                _merchantGroupUse = value;
                MarkValueTypePropertyAsDirty(nameof(MerchantGroupUse));
            }
        }

        public static class ByOwner
        {
            public static string GeneratePartitionKey(string ownerId)
            {
                if(!ownerId.IsValidPartitionOrRowKey())
                    throw new InvalidRowKeyValueException(nameof(ownerId), ownerId);

                return ownerId;
            }

            public static string GenerateRowKey(string id = null)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    if (!id.IsValidPartitionOrRowKey())
                        throw new InvalidRowKeyValueException(nameof(id), id);
                }

                return id ?? Guid.NewGuid().ToString("D");
            }

            public static MerchantGroupEntity Create(IMerchantGroup src)
            {
                var entity = new MerchantGroupEntity
                {
                    PartitionKey = GeneratePartitionKey(src.OwnerId),
                    RowKey = GenerateRowKey()
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
                return "IdIndex";
            }

            public static AzureIndex Create(MerchantGroupEntity entity)
            {
                return AzureIndex.Create(
                    GeneratePartitionKey(entity.Id),
                    GenerateRowKey(), entity);
            }
        }
    }
}
