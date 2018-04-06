using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Wallet;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IBcnWalletUsageService
    {
        Task<IBcnWalletUsage> OccupyAsync(string walletAddress, BlockchainType blockchain, string occupiedBy);

        Task<IBcnWalletUsage> ReleaseAsync(string walletAddress, BlockchainType blockchain);

        Task<IBcnWalletUsage> GetAsync(string walletAddress, BlockchainType blockchain);

        Task<string> ResolveOccupierAsync(string walletAddress, BlockchainType blockchain);
    }
}
