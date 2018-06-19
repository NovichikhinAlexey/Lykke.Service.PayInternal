using System.Collections.Generic;
using System.Linq;
using Lykke.Service.PayInternal.Core.Domain;

namespace Lykke.Service.PayInternal.Services
{
    public static class BalanceExtensions
    {
        public static BlockchainBalanceResult For(this IReadOnlyList<BlockchainBalanceResult> src, string assetId)
        {
            return src.SingleOrDefault(x => x.AssetId == assetId);
        }
    }
}
