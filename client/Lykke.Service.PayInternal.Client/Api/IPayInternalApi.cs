using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Transactions;
using Lykke.Service.PayInternal.Client.Models.Wallets;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IPayInternalApi
    {
        [Post("/api/bitcoin/address")]
        Task<WalletAddressResponse> CreateAddressAsync([Body] CreateWalletRequest request);

        [Get("/api/bitcoin/wallets/notExpired")]
        Task<IEnumerable<WalletStateResponse>> GetNotExpiredWalletsAsync();

        [Post("/api/transactions/payment")]
        Task CreatePaymentTransactionAsync([Body] CreateTransactionRequest request);

        [Put("/api/transactions")]
        Task UpdateTransactionAsync([Body] UpdateTransactionRequest request);

        [Get("/api/transactions/GetAllMonitored")]
        Task<IEnumerable<TransactionStateResponse>> GetAllMonitoredTransactionsAsync();

        [Post("/api/transactions/expired")]
        Task SetTransactionExpiredAsync([Body] TransactionExpiredRequest request);
        [Get("/api/transactions/{walletAddress}")]
        Task<IReadOnlyList<string>> GetTransactionsSourceWalletsAsync(string walletAddress);
    }
}
