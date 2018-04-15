using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class VirtualWalletService : IVirtualWalletService
    {
        private readonly IVirtualWalletRepository _virtualWalletRepository;

        public VirtualWalletService(
            IVirtualWalletRepository virtualWalletRepository)
        {
            _virtualWalletRepository = virtualWalletRepository;
        }

        public async Task<IVirtualWallet> CreateAsync(string merchantId, DateTime dueDate, IList<BlockchainWallet> wallets = null)
        {
            return await _virtualWalletRepository.CreateAsync(new VirtualWallet
            {
                MerchantId = merchantId,
                DueDate = dueDate,
                CreatedOn = DateTime.UtcNow,
                BlockchainWallets = wallets
            });
        }

        public async Task<IVirtualWallet> GetAsync(string merchantId, string walletId)
        {
            return await _virtualWalletRepository.GetAsync(merchantId, walletId);
        }

        public async Task<IVirtualWallet> FindAsync(string walletId)
        {
            return await _virtualWalletRepository.FindAsync(walletId);
        }

        public async Task<IVirtualWallet> AddAddressAsync(string merchantId, string walletId, BlockchainWallet blockchainWallet)
        {
            IVirtualWallet wallet = await _virtualWalletRepository.GetAsync(merchantId, walletId);

            if (wallet == null)
                throw new WalletNotFoundException(walletId);

            bool exists = wallet.BlockchainWallets.Any(x =>
                x.Blockchain == blockchainWallet.Blockchain && x.Address == blockchainWallet.Address);

            if (exists)
                throw new WalletAlreadyExistsException(walletId);

            wallet.BlockchainWallets.Add(blockchainWallet);

            await _virtualWalletRepository.SaveAsync(wallet);

            return wallet;
        }

        public async Task<IReadOnlyList<IVirtualWallet>> GetNotExpiredAsync()
        {
            return await _virtualWalletRepository.GetByDueDateAsync(DateTime.UtcNow);
        }
    }
}
