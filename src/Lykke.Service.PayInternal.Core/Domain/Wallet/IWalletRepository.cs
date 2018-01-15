using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public interface IWalletRepository
    {
        Task SaveAsync(IWallet wallet);

        Task<IEnumerable<IWallet>> GetAsync();

        Task<IWallet> GetAsync(string merchantId, string address);

        Task<IEnumerable<IWallet>> GetByMerchantAsync(string merchantId, bool nonEmptyOnly = false);
    }
}
