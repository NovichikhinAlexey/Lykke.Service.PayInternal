using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public interface IWalletRepository
    {
        Task SaveAsync(IWallet wallet);

        Task<IEnumerable<IWallet>> GetAsync();

        Task<IEnumerable<IWallet>> GetByMerchantAsync(string merchantId);
    }
}
