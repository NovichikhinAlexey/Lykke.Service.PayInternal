using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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

        public async Task<Core.Domain.Transfer.Transfer> AddAsync(ITransfer transfer)
        {
            var entity = TransferEntity.ByDate.Create(transfer);

            await _storage.InsertAsync(entity);

            return Mapper.Map<Core.Domain.Transfer.Transfer>(entity);
        }

        //todo: add indexes
        public async Task<Core.Domain.Transfer.Transfer> GetAsync(string id)
        {
            IList<TransferEntity> entities = await _storage.GetDataAsync(t => t.Id.Equals(id));

            return Mapper.Map<Core.Domain.Transfer.Transfer>(entities.SingleOrDefault());
        }
    }
}
