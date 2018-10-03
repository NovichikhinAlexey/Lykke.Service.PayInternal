using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using Lykke.Service.PayInternal.AzureRepositories.Extensions;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.PayInternal.AzureRepositories.PaymentRequest
{
    public class PaymentRequestRepository : IPaymentRequestRepository
    {
        private readonly INoSQLTableStorage<PaymentRequestEntity> _storage;
        private readonly INoSQLTableStorage<AzureIndex> _walletAddressIndexStorage;
        private readonly INoSQLTableStorage<AzureIndex> _dueDateIndexStorage;
        // used only for nameof
        private const PaymentRequestEntity Entity = null;

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

        public async Task<bool> HasAnyPaymentRequestAsync(string merchantId)
        {
            var entity = await _storage.GetTopRecordAsync(PaymentRequestEntity.ByMerchant.GeneratePartitionKey(merchantId));
            return entity != null;
        }

        public async Task<IReadOnlyList<IPaymentRequest>> GetByFilterAsync(PaymentsFilter paymentsFilter)
        {
            var filter = nameof(Entity.PartitionKey)
                .PropertyEqual(PaymentRequestEntity.ByMerchant.GeneratePartitionKey(paymentsFilter.MerchantId))
                .And(nameof(Entity.Initiator).PropertyEqual(LykkePayConstants.ApiPaymentRequestInitiator));
            
            if(paymentsFilter.Statuses.Any())
            {
                var localFilter = string.Empty;
                foreach (var status in paymentsFilter.Statuses)
                {
                    localFilter = localFilter.OrIfNotEmpty(nameof(Entity.Status).PropertyEqual(status.ToString()));
                }
                filter = filter.AndIfNotEmpty(localFilter);
            }

            if(paymentsFilter.ProcessingErrors.Any())
            {
                var localFilter = string.Empty;
                foreach (var processingError in paymentsFilter.ProcessingErrors)
                {
                    localFilter = localFilter.OrIfNotEmpty(nameof(Entity.ProcessingError).PropertyEqual(processingError.ToString()));
                }
                filter = filter.AndIfNotEmpty(localFilter);
            }

            if (paymentsFilter.DateFrom.HasValue)
            {
                filter = filter.AndIfNotEmpty(nameof(Entity.CreatedOn).DateGreaterThanOrEqual(paymentsFilter.DateFrom.Value));
            }

            if (paymentsFilter.DateTo.HasValue)
            {
                filter = filter.AndIfNotEmpty(nameof(Entity.CreatedOn).DateLessThanOrEqual(paymentsFilter.DateTo.Value));
            }

            var tableQuery = new TableQuery<PaymentRequestEntity>().Where(filter);

            var result = await _storage.WhereAsync(tableQuery);

            return Mapper.Map<List<Core.Domain.PaymentRequests.PaymentRequest>>(result);
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
