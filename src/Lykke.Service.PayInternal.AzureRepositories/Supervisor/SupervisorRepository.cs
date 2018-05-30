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
        public SupervisorRepository(
            INoSQLTableStorage<SupervisorEntity> storage)
        {
            _storage = storage;
        }

        public async Task<ISupervisor> GetAsync(string merchantId, string employeeId)
        {
            var entity = await _storage.GetDataAsync(
                SupervisorEntity.IndexByEmployee.GeneratePartitionKey(merchantId),
                SupervisorEntity.IndexByEmployee.GenerateRowKey(employeeId));

            return Mapper.Map<Core.Domain.Supervisor.Supervisor>(entity);
        }

        public async Task<ISupervisor> InsertAsync(ISupervisor supervisor)
        {
            var entity = new SupervisorEntity(SupervisorEntity.IndexByEmployee.GeneratePartitionKey(supervisor.MerchantId), SupervisorEntity.IndexByEmployee.GenerateRowKey(supervisor.EmployeeId));
            Mapper.Map(supervisor, entity);
            await _storage.InsertAsync(entity);

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
        public async Task DeleteAsync(string merchantId, string employeeId)
        {
            await _storage.DeleteAsync(SupervisorEntity.IndexByEmployee.GeneratePartitionKey(merchantId), SupervisorEntity.IndexByEmployee.GenerateRowKey(employeeId));
        }
    }
}
