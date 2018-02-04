using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.AzureRepositories.Transfer
{
    public class TransferRepository : ITransferRepository
    {
        private readonly INoSQLTableStorage<TransferEntity> _tableStorage;
        public TransferRepository(INoSQLTableStorage<TransferEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }
        public async Task<IEnumerable<ITransferRequest>> GetAllAsync()
        {
            var transactions = await _tableStorage.GetDataAsync();
            return AggregateTransactions(transactions);
        }

    

    public async Task<ITransferRequest> GetAsync(string transferRequestId)
        {
            var transactions = await _tableStorage.GetDataAsync(transferRequestId);
            return AggregateTransactions(transactions.ToList()).FirstOrDefault();
        }

        public async Task<ITransferRequest> GetAsync(string transferRequestId, string transactionHash)
        {
            var transaction = await _tableStorage.GetDataAsync(transferRequestId, transactionHash);
            return AggregateTransactions((new []{ transaction }).ToList()).FirstOrDefault(); 
        }

        public async Task<ITransferRequest> SaveAsync(ITransferRequest transferInfo)
        {
            var transfers = TransferEntity.Create(transferInfo);

            foreach (var transfer in transfers)
            {
                try
                {
                    await _tableStorage.InsertOrMergeAsync(transfer);

                }
                catch
                {
                    return null;
                }
            }

            return transferInfo;
        }

        private List<ITransferRequest> AggregateTransactions(IList<TransferEntity> transactions)
        {
            var transferAgg = (from transaction in transactions
                group transaction by transaction.TransferId
                into transferAggregation
                select transferAggregation).ToList();

            return (from transfer in transferAgg
                    let transferTransactions = transfer.ToList()
                    select (ITransferRequest) new TransferRequest
                    {
                        TransferId = transfer.Key,
                        TransferStatus = transferTransactions.First().TransferStatus,
                        TransferStatusError = transferTransactions.First().TransferStatusError,
                        CreateDate = transferTransactions.First().CreatedDate,
                        TransactionRequests = (from tt in transferTransactions
                                              select (ITransactionRequest) new TransactionRequest
                                              {
                                                  Amount = tt.Amount,
                                                  Currency = tt.Currency,
                                                  DestinationAddress = tt.DestinationAddress,
                                                  TransactionHash = tt.TransactionHash,
                                                  CountConfirm = tt.CountConfirm,
                                                  SourceAmounts = new List<ISourceAmount>(tt.SourceAddresses)

                                              }).ToList()
                    }).ToList();
        }
    }
}
