using AutoMapper;
using AzureStorage.Tables.Templates.Index;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core.Domain.SupervisorMembership;

namespace Lykke.Service.PayInternal.AzureRepositories.SupervisorMembership
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class SupervisorMembershipEntity : AzureTableEntity
    {
        public string MerchantId { get; set; }
        public string EmployeeId { get; set; }
        public string MerchantGroups { get; set; }

        public static class ByMerchant
        {
            public static string GeneratePartitionKey(string merchantId)
            {
                return merchantId;
            }

            public static string GenerateRowKey(string employeeId)
            {
                return employeeId;
            }

            public static SupervisorMembershipEntity Create(ISupervisorMembership src)
            {
                var entity = new SupervisorMembershipEntity
                {
                    PartitionKey = GeneratePartitionKey(src.MerchantId),
                    RowKey = GenerateRowKey(src.EmployeeId)
                };

                return Mapper.Map(src, entity);
            }
        }

        public static class IndexByEmployee
        {
            public static string GeneratePartitionKey(string employeeId)
            {
                return employeeId;
            }

            public static string GenerateRowKey()
            {
                return "EmployeeIdIndex";
            }

            public static AzureIndex Create(SupervisorMembershipEntity membershipEntity)
            {
                return AzureIndex.Create(
                    GeneratePartitionKey(membershipEntity.EmployeeId),
                    GenerateRowKey(), membershipEntity);
            }
        }
    }
}
