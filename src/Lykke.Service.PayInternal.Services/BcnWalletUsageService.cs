using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class BcnWalletUsageService : IBcnWalletUsageService
    {
        private readonly IBcnWalletUsageRepository _walletUsageRepository;
        public BcnWalletUsageService(IBcnWalletUsageRepository walletUsageRepository)
        {
            _walletUsageRepository = walletUsageRepository;
        }

        public async Task<IBcnWalletUsage> OccupyAsync(string walletAddress, BlockchainType blockchain, string occupiedBy)
        {
            BcnWalletUsage usage = new BcnWalletUsage
            {
                Blockchain = blockchain,
                WalletAddress = walletAddress,
                OccupiedBy = occupiedBy
            };

            bool isLocked = await _walletUsageRepository.TryLockAsync(usage);
            
            if (!isLocked)
                throw new WalletAddressInUseException(walletAddress, blockchain);

            return usage;
        }

        public async Task<IBcnWalletUsage> ReleaseAsync(string walletAddress, BlockchainType blockchain)
        {
            return await _walletUsageRepository.ReleaseAsync(walletAddress, blockchain);
        }

        public async Task<IBcnWalletUsage> GetAsync(string walletAddress, BlockchainType blockchain)
        {
            return await _walletUsageRepository.GetAsync(walletAddress, blockchain);
        }

        public async Task<string> ResolveOccupierAsync(string walletAddress, BlockchainType blockchain)
        {
            IBcnWalletUsage usage = await _walletUsageRepository.GetAsync(walletAddress, blockchain);

            return usage?.OccupiedBy;
        }
    }
}
