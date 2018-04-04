using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public interface IVirtualWalletRepository
    {
        Task<IVirtualWallet> CreateAsync(IVirtualWallet wallet);
    }
}
