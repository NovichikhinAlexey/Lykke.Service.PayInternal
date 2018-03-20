using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Microsoft.WindowsAzure.Storage.Table;

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

            return Mapper.Map<IEnumerable<Core.Domain.PaymentRequests.PaymentRequest>>(paymentRequestEntities).ToList();
        }

        public async Task<IPaymentRequest> GetAsync(string merchantId, string paymentRequestId)
        {
            PaymentRequestEntity entity =
                await _storage.GetDataAsync(GetPartitionKey(merchantId), GetRowKey(paymentRequestId));

            return Mapper.Map<Core.Domain.PaymentRequests.PaymentRequest>(entity);
        }

        public async Task<IPaymentRequest> FindAsync(string walletAddress)
        {
            AzureIndex index = await _walletAddressIndexStorage
                .GetDataAsync(GetWalletAddressIndexPartitionKey(walletAddress), GetWalletAddressIndexRowKey());

            if (index == null)
                return null;

            PaymentRequestEntity entity = await _storage.GetDataAsync(index);

            return Mapper.Map<Core.Domain.PaymentRequests.PaymentRequest>(entity);
        }

        public async Task<IEnumerable<IPaymentRequest>> GetNotExpiredAsync()
        {
            var filter =
                TableQuery.GenerateFilterConditionForDate("DueDate", QueryComparisons.GreaterThanOrEqual, DateTimeOffset.UtcNow);

            var query = new TableQuery<PaymentRequestEntity>().Where(filter);

            IEnumerable<PaymentRequestEntity> entities = await _storage.WhereAsync(query);

            return Mapper.Map<IEnumerable<Core.Domain.PaymentRequests.PaymentRequest>>(entities);
        }

        public async Task<IPaymentRequest> InsertAsync(IPaymentRequest paymentRequest)
        {
            var entity = new PaymentRequestEntity(GetPartitionKey(paymentRequest.MerchantId), GetRowKey());

            Mapper.Map(paymentRequest, entity);

            await _storage.InsertAsync(entity);

            var index = AzureIndex.Create(GetWalletAddressIndexPartitionKey(entity.WalletAddress), GetWalletAddressIndexRowKey(), entity);
            
            await _walletAddressIndexStorage.InsertAsync(index);
            
            return Mapper.Map<Core.Domain.PaymentRequests.PaymentRequest>(entity);
        }

        public async Task UpdateAsync(IPaymentRequest paymentRequest)
        {
            var entity = new PaymentRequestEntity(GetPartitionKey(paymentRequest.MerchantId), GetRowKey(paymentRequest.Id));

            Mapper.Map(paymentRequest, entity);

            entity.ETag = "*";
            
            await _storage.ReplaceAsync(entity);
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
