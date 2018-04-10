using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public interface IBcnWalletUsageRepository
    {
        Task<IBcnWalletUsage> CreateAsync(IBcnWalletUsage usage);

        Task<IBcnWalletUsage> GetAsync(string walletAddress, BlockchainType blockchain);

        Task<IList<IBcnWalletUsage>> GetVacantAsync(BlockchainType blockchain);

        Task<bool> TryLockAsync(IBcnWalletUsage usage);

        Task<IBcnWalletUsage> ReleaseAsync(string walletAddress, BlockchainType blockchain);
    }
}
