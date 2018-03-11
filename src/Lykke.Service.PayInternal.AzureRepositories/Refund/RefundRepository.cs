using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Refund;

namespace Lykke.Service.PayInternal.AzureRepositories.Refund
{
    public class RefundRepository : IRefundRepository
    {
        private readonly INoSQLTableStorage<RefundEntity> _storage;

        public RefundRepository(INoSQLTableStorage<RefundEntity> storage)
        {
            _storage = storage;
        }

        public async Task AddAsync(IRefund refund)
        {
            var entity = new RefundEntity();
            entity.Map(refund);

            await _storage.InsertAsync(entity);
        }

        public async Task<IRefund> GetAsync(string merchantId, string refundId)
        {
            return await _storage.GetDataAsync(merchantId, refundId);
        }
    }
}
