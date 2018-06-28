using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.MerchantWallet
{
    public interface IMerchantWalletRespository
    {
        Task<IMerchantWallet> CreateAsync(IMerchantWallet src);

        Task<IMerchantWallet> GetByAddressAsync(BlockchainType network, string walletAddress);

        Task<IMerchantWallet> GetByIdAsync(string id);

        Task<IReadOnlyList<IMerchantWallet>> GetByMerchantAsync(string merchantId);

        Task UpdateAsync(IMerchantWallet src);

        Task DeleteAsync(string merchantId, BlockchainType network, string walletAddress);

        Task DeleteAsync(string merchantWalletId);
    }
}
