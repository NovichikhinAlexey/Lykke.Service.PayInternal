using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.MerchantWallet;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IMerchantWalletService
    {
        Task<IMerchantWallet> CreateAsync(CreateMerchantWalletCommand cmd);

        Task DeleteAsync(string merchantId, BlockchainType network, string walletAddress);

        Task DeleteAsync(string merchantWalletId);

        Task SetDefaultAssetsAsync(
            string merchantId, 
            BlockchainType network, 
            string walletAddress,
            IEnumerable<string> incomingPaymentDefaults = null, 
            IEnumerable<string> outgoingPaymentDefaults = null);

        Task<IMerchantWallet> GetDefaultAsync(string merchantId, string assetId, PaymentDirection paymentDirection);

        Task<IReadOnlyList<IMerchantWallet>> GetByMerchantAsync(string merchantId);

        Task<IReadOnlyList<IMerchantWalletBalance>> GetBalancesAsync();

        Task<IMerchantWalletBalance> GetBalanceAsync(string merchantId, BlockchainType network, string walletAddress);
    }
}
