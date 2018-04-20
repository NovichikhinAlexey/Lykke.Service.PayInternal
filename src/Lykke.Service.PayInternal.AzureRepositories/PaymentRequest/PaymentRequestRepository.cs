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
        private readonly INoSQLTableStorage<AzureIndex> _dueDateIndexStorage;

        public PaymentRequestRepository(
            INoSQLTableStorage<PaymentRequestEntity> storage,
            INoSQLTableStorage<AzureIndex> walletAddressIndexStorage, 
            INoSQLTableStorage<AzureIndex> dueDateIndexStorage)
        {
            _storage = storage;
            _walletAddressIndexStorage = walletAddressIndexStorage;
            _dueDateIndexStorage = dueDateIndexStorage;
        }
        
        public async Task<IReadOnlyList<IPaymentRequest>> GetAsync(string merchantId)
        {
            IEnumerable<PaymentRequestEntity> paymentRequestEntities
                = await _storage.GetDataAsync(PaymentRequestEntity.ByMerchant.GeneratePartitionKey(merchantId));

            return Mapper.Map<IEnumerable<Core.Domain.PaymentRequests.PaymentRequest>>(paymentRequestEntities).ToList();
        }

        public async Task<IPaymentRequest> GetAsync(string merchantId, string paymentRequestId)
        {
            PaymentRequestEntity entity = await _storage.GetDataAsync(
                    PaymentRequestEntity.ByMerchant.GeneratePartitionKey(merchantId),
                    PaymentRequestEntity.ByMerchant.GenerateRowKey(paymentRequestId));

            return Mapper.Map<Core.Domain.PaymentRequests.PaymentRequest>(entity);
        }

        public async Task<IPaymentRequest> FindAsync(string walletAddress)
        {
            AzureIndex index = await _walletAddressIndexStorage.GetDataAsync(
                PaymentRequestEntity.IndexByWallet.GeneratePartitionKey(walletAddress),
                PaymentRequestEntity.IndexByWallet.GenerateRowKey());

            if (index == null) return null;

            PaymentRequestEntity entity = await _storage.GetDataAsync(index);

            return Mapper.Map<Core.Domain.PaymentRequests.PaymentRequest>(entity);
        }

        public async Task<IReadOnlyList<IPaymentRequest>> GetByDueDate(DateTime from, DateTime to)
        {
            string gtDate = PaymentRequestEntity.IndexByDueDate.GeneratePartitionKey(from);

            string ltDate = PaymentRequestEntity.IndexByDueDate.GeneratePartitionKey(to);

            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, gtDate),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThan, ltDate));

            var query = new TableQuery<AzureIndex>().Where(filter);

            IEnumerable<AzureIndex> indecies = await _dueDateIndexStorage.WhereAsync(query);

            IEnumerable<PaymentRequestEntity> entities = await _storage.GetDataAsync(indecies);

            return Mapper.Map<IEnumerable<Core.Domain.PaymentRequests.PaymentRequest>>(entities).ToList();
        }

        public async Task<IPaymentRequest> InsertAsync(IPaymentRequest paymentRequest)
        {
            var entity = PaymentRequestEntity.ByMerchant.Create(paymentRequest);

            await _storage.InsertAsync(entity);

            var indexByWallet = PaymentRequestEntity.IndexByWallet.Create(entity);
            
            await _walletAddressIndexStorage.InsertAsync(indexByWallet);

            var indexByDueDate = PaymentRequestEntity.IndexByDueDate.Create(entity);

            await _dueDateIndexStorage.InsertAsync(indexByDueDate);
            
            return Mapper.Map<Core.Domain.PaymentRequests.PaymentRequest>(entity);
        }

        public async Task UpdateAsync(IPaymentRequest paymentRequest)
        {
            await _storage.MergeAsync(
                PaymentRequestEntity.ByMerchant.GeneratePartitionKey(paymentRequest.MerchantId),
                PaymentRequestEntity.ByMerchant.GenerateRowKey(paymentRequest.Id),
                entity =>
                {
                    entity.Amount = paymentRequest.Amount;
                    entity.OrderId = paymentRequest.OrderId;
                    entity.PaidAmount = paymentRequest.PaidAmount;
                    entity.PaidDate = paymentRequest.PaidDate;
                    entity.ProcessingError = paymentRequest.ProcessingError;
                    entity.Status = paymentRequest.Status;

                    return entity;
                });
        }
    }
}
