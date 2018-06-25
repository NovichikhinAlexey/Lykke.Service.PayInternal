using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Transactions;
using Lykke.Service.PayInternal.Client.Models.Transactions.Ethereum;
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

        [Post("/api/transactions/payment/lykke")]
        Task CreateLykkePaymentTransactionAsync([Body] CreateLykkeTransactionRequest request);

        [Put("/api/transactions")]
        Task UpdateTransactionAsync([Body] UpdateTransactionRequest request);

        [Get("/api/transactions/GetAllMonitored")]
        Task<IEnumerable<TransactionStateResponse>> GetAllMonitoredTransactionsAsync();

        [Post("/api/transactions/expired")]
        Task SetTransactionExpiredAsync([Body] TransactionExpiredRequest request);


        [Post("/api/wallets/expired")]
        Task SetWalletExpiredAsync([Body] BlockchainWalletExpiredRequest request);

        [Post("/api/ethereumTransactions/inbound")]
        Task RegisterEthereumInboundTransactionAsync([Body] RegisterInboundTxModel request);

        [Post("/api/ethereumTransactions/outbound")]
        Task RegisterEthereumOutboundTransactionAsync([Body] RegisterOutboundTxModel request);

        [Post("/api/ethereumTransactions/outbound/complete")]
        Task CompleteEthereumOutboundTransactionAsync([Body] CompleteOutboundTxModel request);

        [Post("/api/ethereumTransactions/outbound/fail")]
        Task FailEthereumOutboundTransactionAsync([Body] FailOutboundTxModel request);
    }
}
