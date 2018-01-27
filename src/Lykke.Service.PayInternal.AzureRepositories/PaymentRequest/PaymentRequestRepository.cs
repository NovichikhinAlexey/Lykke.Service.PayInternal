using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;

namespace Lykke.Service.PayInternal.AzureRepositories.PaymentRequest
{
    public class PaymentRequestRepository : IPaymentRequestRepository
    {
        private readonly INoSQLTableStorage<PaymentRequestEntity> _storage;
        private readonly INoSQLTableStorage<AzureIndex> _walletAddressIndexStorage;

        public PaymentRequestRepository(
            INoSQLTableStorage<PaymentRequestEntity> storage,
            INoSQLTableStorage<AzureIndex> walletAddressIndexStorage)
        {
            _storage = storage;
            _walletAddressIndexStorage = walletAddressIndexStorage;
        }
        
        public async Task<IReadOnlyList<IPaymentRequest>> GetAsync(string merchantId)
        {
            IEnumerable<PaymentRequestEntity> paymentRequestEntities
                = await _storage.GetDataAsync(GetPartitionKey(merchantId));

            return paymentRequestEntities.ToList();
        }

        public async Task<IPaymentRequest> GetAsync(string merchantId, string paymentRequestId)
        {
            return await _storage.GetDataAsync(GetPartitionKey(merchantId), GetRowKey(paymentRequestId));
        }

        public async Task<IPaymentRequest> FindAsync(string walletAddress)
        {
            AzureIndex index = await _walletAddressIndexStorage
                .GetDataAsync(GetWalletAddressIndexPartitionKey(walletAddress), GetWalletAddressIndexRowKey());

            if (index == null)
                return null;

            return await _storage.GetDataAsync(index);
        }

        public async Task<IPaymentRequest> InsertAsync(IPaymentRequest paymentRequest)
        {
            var entity = new PaymentRequestEntity(GetPartitionKey(paymentRequest.MerchantId), GetRowKey());
            entity.Map(paymentRequest);

            await _storage.InsertAsync(entity);

            var index = AzureIndex.Create(GetWalletAddressIndexPartitionKey(entity.WalletAddress), GetWalletAddressIndexRowKey(), entity);
            
            await _walletAddressIndexStorage.InsertAsync(index);
            
            return entity;
        }
        
        private static string GetPartitionKey(string merchantId)
            => merchantId;

        private static string GetRowKey(string paymentRequestId)
            => paymentRequestId;

        private static string GetRowKey()
            => Guid.NewGuid().ToString("D");
        
        private static string GetWalletAddressIndexPartitionKey(string walletAddress)
            => walletAddress;

        private static string GetWalletAddressIndexRowKey()
            => "WalletAddressIndex";
    }
}
