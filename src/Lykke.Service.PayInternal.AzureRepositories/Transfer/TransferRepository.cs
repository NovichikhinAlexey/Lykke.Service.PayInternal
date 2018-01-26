using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.AzureRepositories.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Microsoft.WindowsAzure.Storage;

namespace Lykke.Service.PayInternal.AzureRepositories.Transfer
{
    public class TransferRepository : ITransferRepository
    {
        private readonly INoSQLTableStorage<TransferEntity> _tableStorage;
        public TransferRepository(INoSQLTableStorage<TransferEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }
        public async Task<IEnumerable<ITransferInfo>> GetAllAsync()
        {
            var result = await _tableStorage.GetDataAsync();
            return result.Cast<ITransferInfo>().ToList();
        }

    

    public async Task<IEnumerable<ITransferInfo>> GetAsync(string transferRequestId)
        {
            var result = await _tableStorage.GetDataAsync(transferRequestId);
            return result.Cast<ITransferInfo>().ToList();
        }

        public async Task<ITransferInfo> GetAsync(string transferRequestId, string transactionHash)
        {
            var result = await _tableStorage.GetDataAsync(transferRequestId, transactionHash);
            return result;
        }

        public async Task<ITransferInfo> SaveAsync(ITransferInfo transferInfo)
        {
           var ti = new TransferEntity(transferInfo);
            try
            {
                await _tableStorage.InsertOrMergeAsync(ti);

            }
            catch
            {
                return null;
            }
            return ti;
        }
    }
}
