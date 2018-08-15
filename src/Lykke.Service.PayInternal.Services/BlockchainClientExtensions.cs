using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public static class BlockchainClientExtensions
    {
        public static async Task<decimal> GetBalanceAsync(this IBlockchainApiClient src, string address, string assetId)
        {
            return (await src.GetBalancesAsync(address)).For(assetId)?.Balance ?? 0;
        }
    }
}
