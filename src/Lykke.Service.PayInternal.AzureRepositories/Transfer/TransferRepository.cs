using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.AzureRepositories.Transfer
{
    public class TransferRepository : ITransferRepository
    {
        private readonly INoSQLTableStorage<TransferEntity> _storage;
        private readonly INoSQLTableStorage<AzureIndex> _transferIdIndexStorage;

        public TransferRepository(
            INoSQLTableStorage<TransferEntity> storage, 
            INoSQLTableStorage<AzureIndex> transferIdIndexStorage)
        {
            _storage = storage;
            _transferIdIndexStorage = transferIdIndexStorage;
        }

        public async Task<Core.Domain.Transfer.Transfer> AddAsync(ITransfer transfer)
        {
            TransferEntity entity = TransferEntity.ByDate.Create(transfer);

            await _storage.InsertAsync(entity);

            AzureIndex indexById = TransferEntity.IndexById.Create(entity);

            await _transferIdIndexStorage.InsertAsync(indexById);

            return Mapper.Map<Core.Domain.Transfer.Transfer>(entity);
        }

        public async Task<Core.Domain.Transfer.Transfer> GetAsync(string id)
        {
            AzureIndex index = await _transferIdIndexStorage.GetDataAsync(
                TransferEntity.IndexById.GeneratePartitionKey(id), 
                TransferEntity.IndexById.GenerateRowKey());

            if (index == null) return null;

            TransferEntity entity = await _storage.GetDataAsync(index);

            return Mapper.Map<Core.Domain.Transfer.Transfer>(entity);
        }
    }
}
