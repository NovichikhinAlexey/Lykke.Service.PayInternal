using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Wallet;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IVirtualWalletService
    {
        Task<IVirtualWallet> CreateAsync(string merchantId, DateTime dueDate, IList<BlockchainWallet> wallets = null);

        Task<IVirtualWallet> GetAsync(string merchantId, string walletId);

        Task<IVirtualWallet> AddAddressAsync(string merchantId, string walletId, BlockchainWallet blockchainWallet);

        Task<IReadOnlyList<IVirtualWallet>> GetNotExpiredAsync();
    }
}
