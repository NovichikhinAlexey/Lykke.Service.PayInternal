using AzureStorage.Tables.Templates.Index;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core.Domain.Supervisor;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.AzureRepositories.Supervisor
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class SupervisorEntity : AzureTableEntity, ISupervisor
    {
        public string Id => RowKey;
        public string MerchantId { get; set; }
        public string EmployeeId { get; set; }
        public string MerchantGroups { get; set; }
        public string SupervisorMerchants { get; set; }

        public SupervisorEntity()
        {

        }
        public SupervisorEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
        public static class IndexByEmployee
        {
            public static string GeneratePartitionKey(string merchantId)
            {
                return merchantId;
            }

            public static string GenerateRowKey(string employeeId)
            {
                return employeeId;
            }
            public static string GenerateRowKey()
            {
                return "EmployeeIdIndex";
            }
            public static AzureIndex Create(SupervisorEntity entity)
            {
                return AzureIndex.Create(
                    GeneratePartitionKey(entity.EmployeeId),
                    GenerateRowKey(), entity);
            }
        }
    }
}
