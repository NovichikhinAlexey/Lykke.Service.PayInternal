using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models;

namespace Lykke.Service.PayInternal.Client
{
    public interface IPayInternalClient
    {
        Task<WalletAddressResponse> CreateAddressAsync(CreateWalletRequest request);

        Task<IEnumerable<WalletStateResponse>> GetNotExpiredWalletsAsync();

        Task CreateMerchantAsync(CreateMerchantRequest request);

        Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request);

        Task<CreateOrderResponse> ReCreateOrderAsync(ReCreateOrderRequest request);

        Task UpdatePublicKeyAsync(byte[] content, string id, string fileName, string contentType);

        Task CreateTransaction(CreateTransactionRequest request);

        Task UpdateTransaction(UpdateTransactionRequest request);
    }
}
