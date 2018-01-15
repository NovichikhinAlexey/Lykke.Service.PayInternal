using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Wallet;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IMerchantWalletsService
    {
        Task<string> CreateAddress(ICreateWalletRequest request);
        Task<IWallet> GetAsync(string merchantId, string address);
        Task<IEnumerable<IWallet>> GetAsync(string merchantId);
        Task<IEnumerable<IWallet>> GetNonEmptyAsync(string merchantId);
    }
}
