using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.AzureRepositories.Transfer
{
    public class TransferRepository : ITransferRepository
    {
        private readonly INoSQLTableStorage<TransferEntity> _storage;

        public TransferRepository(INoSQLTableStorage<TransferEntity> storage)
        {
            _storage = storage;
        }

        public async Task<ITransfer> AddAsync(ITransfer transfer)
        {
            var entity = TransferEntity.ByDate.Create(transfer);

            await _storage.InsertAsync(entity);

            return entity;
        }
    }
}
