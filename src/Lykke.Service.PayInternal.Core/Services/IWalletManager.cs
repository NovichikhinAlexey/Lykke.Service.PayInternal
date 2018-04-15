using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Wallet;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IWalletManager
    {
        Task<IVirtualWallet> CreateAsync(string merchantId, DateTime dueDate, string assetId = null);

        Task<IVirtualWallet> AddAssetAsync(string merchantId, string walletId, string assetId);

        Task<IEnumerable<IWalletState>> GetNotExpiredStateAsync();

        Task<string> ResolveBlockchainAddressAsync(string virtualAddress, string assetId);
    }
}
