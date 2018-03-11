using System;
using System.Collections.Generic;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.AzureRepositories.Transfer
{
    public class TransferRepository : ITransferRepository
    {
        private readonly INoSQLTableStorage<TransferEntity> _storage;

        public TransferRepository(INoSQLTableStorage<TransferEntity> storage)
        {
            _storage = storage;
        }

        public async Task AddAsync(IMultipartTransfer transfer)
        {
            var entity = new TransferEntity();
            entity.Map(transfer);

            await _storage.InsertAsync(entity);
        }

        public async Task<IEnumerable<IMultipartTransfer>> GetFiltered(Func<IMultipartTransfer, bool> filter)
        {
            return await _storage.GetDataAsync(filter);
        }
    }
}
