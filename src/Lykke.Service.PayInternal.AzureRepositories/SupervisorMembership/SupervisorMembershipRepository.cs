using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain.SupervisorMembership;
using Lykke.Service.PayInternal.Core.Exceptions;

namespace Lykke.Service.PayInternal.AzureRepositories.SupervisorMembership
{
    public class SupervisorMembershipRepository : ISupervisorMembershipRepository
    {
        private readonly INoSQLTableStorage<SupervisorMembershipEntity> _tableStorage;

        private readonly INoSQLTableStorage<AzureIndex> _employeeIndexStorage;

        public SupervisorMembershipRepository(
            [NotNull] INoSQLTableStorage<SupervisorMembershipEntity> storage,
            [NotNull] INoSQLTableStorage<AzureIndex> employeeIndexStorage)
        {
            _tableStorage = storage ?? throw new ArgumentNullException(nameof(storage));
            _employeeIndexStorage = employeeIndexStorage ?? throw new ArgumentNullException(nameof(employeeIndexStorage));
        }

        public async Task<ISupervisorMembership> GetAsync(string employeeId)
        {
            AzureIndex index = await _employeeIndexStorage.GetDataAsync(
                SupervisorMembershipEntity.IndexByEmployee.GeneratePartitionKey(employeeId),
                SupervisorMembershipEntity.IndexByEmployee.GenerateRowKey());

            if (index == null) return null;

            var entity = await _tableStorage.GetDataAsync(index);

            return Mapper.Map<Core.Domain.SupervisorMembership.SupervisorMembership>(entity);
        }

        public async Task<ISupervisorMembership> AddAsync(ISupervisorMembership src)
        {
            SupervisorMembershipEntity entity = SupervisorMembershipEntity.ByMerchant.Create(src);

            await _tableStorage.InsertThrowConflict(entity);

            AzureIndex index = SupervisorMembershipEntity.IndexByEmployee.Create(entity);

            await _employeeIndexStorage.InsertThrowConflict(index);

            return Mapper.Map<Core.Domain.SupervisorMembership.SupervisorMembership>(entity);
        }

        public async Task UpdateAsync(ISupervisorMembership src)
        {
            SupervisorMembershipEntity updatedEntity = await _tableStorage.MergeAsync(
                SupervisorMembershipEntity.ByMerchant.GeneratePartitionKey(src.MerchantId),
                SupervisorMembershipEntity.ByMerchant.GenerateRowKey(src.EmployeeId),
                entity =>
                {
                    if (src.MerchantGroups.Any())
                        entity.MerchantGroups = string.Join(Constants.Separator, src.MerchantGroups);

                    return entity;
                });

            if (updatedEntity == null)
                throw new KeyNotFoundException();
        }

        public async Task RemoveAsync(string employeeId)
        {
            AzureIndex index = await _employeeIndexStorage.GetDataAsync(
                SupervisorMembershipEntity.IndexByEmployee.GeneratePartitionKey(employeeId),
                SupervisorMembershipEntity.IndexByEmployee.GenerateRowKey());

            if (index == null)
                throw new KeyNotFoundException();

            await _employeeIndexStorage.DeleteAsync(index);

            await _tableStorage.DeleteAsync(index.PrimaryPartitionKey, index.PrimaryRowKey);
        }
    }
}
