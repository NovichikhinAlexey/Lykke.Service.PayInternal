using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Transactions;
using Lykke.Service.PayInternal.Client.Models.Wallets;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IPayInternalApi
    {
        [Get("/api/wallets/notExpired")]
        Task<IEnumerable<WalletStateResponse>> GetNotExpiredWalletsAsync();

        [Post("/api/transactions/payment")]
        Task CreatePaymentTransactionAsync([Body] CreateTransactionRequest request);

        [Put("/api/transactions")]
        Task UpdateTransactionAsync([Body] UpdateTransactionRequest request);

        [Get("/api/transactions/GetAllMonitored")]
        Task<IEnumerable<TransactionStateResponse>> GetAllMonitoredTransactionsAsync();

        [Post("/api/transactions/expired")]
        Task SetTransactionExpiredAsync([Body] TransactionExpiredRequest request);

        [Post("/api/wallets/expired")]
        Task SetWalletExpiredAsync([Body] BlockchainWalletExpiredRequest request);
    }
}
