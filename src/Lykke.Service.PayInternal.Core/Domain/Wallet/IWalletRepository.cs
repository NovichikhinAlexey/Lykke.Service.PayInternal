using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public interface IWalletRepository
    {
        Task SaveAsync(IWalletEntity wallet);

        Task<IEnumerable<IWalletEntity>> GetAsync();

        Task<IEnumerable<IWalletEntity>> GetByMerchantAsync(string merchantId);
    }
}
