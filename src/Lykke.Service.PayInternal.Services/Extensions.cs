using System.Collections.Generic;
using System.Linq;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    public static class Extensions
    {
        public static WalletAllocationPolicy GetPolicy(this IList<BlockchainWalletAllocationPolicy> src,
            BlockchainType blockchainType)
        {
            BlockchainWalletAllocationPolicy policySetting = src.SingleOrDefault(x => x.Blockchain == blockchainType);

            return policySetting?.WalletAllocationPolicy ?? WalletAllocationPolicy.New;
        }
    }
}
