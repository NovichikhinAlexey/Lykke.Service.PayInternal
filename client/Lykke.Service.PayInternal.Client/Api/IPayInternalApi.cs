using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models;
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
        Task CreatePaymentTransaction([Body] CreateTransactionRequest request);

        [Put("/api/transactions")]
        Task UpdateTransaction([Body] UpdateTransactionRequest request);
    }
}
