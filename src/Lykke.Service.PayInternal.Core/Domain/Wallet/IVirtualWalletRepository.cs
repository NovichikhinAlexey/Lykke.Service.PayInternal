using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public interface IVirtualWalletRepository
    {
        Task<IVirtualWallet> CreateAsync(IVirtualWallet wallet);

        Task<IVirtualWallet> GetAsync(string merchantId, string walletId);

        Task<IVirtualWallet> FindAsync(string walletId);

        Task SaveAsync(IVirtualWallet wallet);

        Task<IReadOnlyList<IVirtualWallet>> GetByDueDateAsync(DateTime dueDateGreaterThan);
    }
}
