using AutoMapper;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using Lykke.Service.PayInternal.Core.Domain.Supervisor;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.AzureRepositories.Supervisor
{
    public class SupervisorRepository : ISupervisorRepository
    {
        private readonly INoSQLTableStorage<SupervisorEntity> _storage;
        private readonly INoSQLTableStorage<AzureIndex> _employeeIndexStorage;
        public SupervisorRepository(
            INoSQLTableStorage<SupervisorEntity> storage,
            INoSQLTableStorage<AzureIndex> employeeIndexStorage)
        {
            _storage = storage;
            _employeeIndexStorage = employeeIndexStorage;
        }

        public async Task<ISupervisor> GetAsync(string employeeId)
        {
            AzureIndex index = await _employeeIndexStorage.GetDataAsync(
                SupervisorEntity.IndexByEmployee.GeneratePartitionKey(employeeId),
                SupervisorEntity.IndexByEmployee.GenerateRowKey());

            if (index == null) return null;
            var entity = await _storage.GetDataAsync(index);
            return Mapper.Map<Core.Domain.Supervisor.Supervisor>(entity);
        }

        public async Task<ISupervisor> InsertAsync(ISupervisor supervisor)
        {
            var entity = new SupervisorEntity(SupervisorEntity.IndexByEmployee.GeneratePartitionKey(supervisor.MerchantId),
                SupervisorEntity.IndexByEmployee.GenerateRowKey(supervisor.EmployeeId));
            Mapper.Map(supervisor, entity);
            await _storage.InsertAsync(entity);
            var index = SupervisorEntity.IndexByEmployee.Create(entity);
            await _employeeIndexStorage.InsertAsync(index);
            
            return entity;
        }
        public async Task<ISupervisor> UpdateAsync(ISupervisor supervisor)
        {
            var entity = new SupervisorEntity(SupervisorEntity.IndexByEmployee.GeneratePartitionKey(supervisor.MerchantId), SupervisorEntity.IndexByEmployee.GenerateRowKey(supervisor.EmployeeId));
            entity.ETag = "*";
            Mapper.Map(supervisor, entity);
            await _storage.MergeAsync(entity.PartitionKey, entity.Id, e => entity);

            return entity;
        }
        public async Task DeleteAsync(string employeeId)
        {
            AzureIndex index = await _employeeIndexStorage.GetDataAsync(
                SupervisorEntity.IndexByEmployee.GeneratePartitionKey(employeeId),
                SupervisorEntity.IndexByEmployee.GenerateRowKey());

            await _storage.DeleteAsync(index);
        }
    }
}
