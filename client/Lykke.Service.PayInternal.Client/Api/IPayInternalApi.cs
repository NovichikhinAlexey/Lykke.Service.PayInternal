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

        [Post("/api/merchants")]
        Task CreateMerchantAsync([Body] CreateMerchantRequest request);

        [Post("/api/orders")]
        Task<CreateOrderResponse> CreateOrderAsync([Body] CreateOrderRequest request);

        [Post("/api/orders/recreate")]
        Task<CreateOrderResponse> ReCreateOrderAsync([Body] ReCreateOrderRequest request);

        [Multipart]
        [Post("/api/merchants/{id}/publicKey")]
        Task UpdatePublicKeyAsync([AliasAs("file")] StreamPart stream, string id);

        [Post("/api/transactions")]
        Task CreateTransaction([Body] CreateTransactionRequest request);

        [Put("/api/transactions")]
        Task UpdateTransaction([Body] UpdateTransactionRequest request);
    }
}
