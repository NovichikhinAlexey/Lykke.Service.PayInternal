using AutoMapper;
using AzureStorage.Tables.Templates.Index;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core.Domain.MerchantGroup;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.AzureRepositories.MerchantGroup
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class MerchantGroupEntity : AzureTableEntity, IMerchantGroup
    {
        public string Id => RowKey;

        public string DisplayName { get; set; }

        public string OwnerId { get; set; }

        public string Merchants { get; set; }

        public MerchantGroupType MerchantGroupType { get; set; }
        public MerchantGroupEntity()
        {
        }
        public MerchantGroupEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
        public static string GenerateRowKey()
        {
            return Guid.NewGuid().ToString();
        }
        public static string GeneratePartitionKey(string ownerId)
        {
            return ownerId;
        }
        public static class IndexByGroup
        {
            public static string GeneratePartitionKey(string groupId)
            {
                return groupId;
            }

            public static string GenerateRowKey()
            {
                return "GroupIdIndex";
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
