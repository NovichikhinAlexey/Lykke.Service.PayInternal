using Lykke.Service.PayInternal.Core.Domain.Wallet;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IMerchantWalletsService
    {
        Task<string> CreateAddress(ICreateWalletRequest request);

        Task<IEnumerable<IWalletState>> GetNotExpiredAsync();
    }
}
