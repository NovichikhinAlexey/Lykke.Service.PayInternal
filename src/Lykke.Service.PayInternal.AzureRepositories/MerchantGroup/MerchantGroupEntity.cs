using AutoMapper;
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

        public string OwnerMerchantId { get; set; }

        public string Merchants { get; set; }

        public AppointmentType Appointment { get; set; }
        public MerchantGroupEntity()
        {
        }
        public MerchantGroupEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
        public static class ByOwner
        {
            public static string GeneratePartitionKey(string merchantId)
            {
                return merchantId;
            }

            public static string GenerateRowKey(string id = null)
            {
                return id ?? Guid.NewGuid().ToString();
            }

            public static MerchantGroupEntity Create(IMerchantGroup merchantGroup)
            {
                var entity = new MerchantGroupEntity
                {
                    PartitionKey = GeneratePartitionKey(merchantGroup.OwnerMerchantId),
                    RowKey = GenerateRowKey()
                };

                return Mapper.Map(merchantGroup, entity);
            }
        }
    }
}
