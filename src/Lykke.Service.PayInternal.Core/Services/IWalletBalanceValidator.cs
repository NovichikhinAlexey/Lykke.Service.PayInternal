using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IWalletBalanceValidator
    {
        Task ValidateTransfer(string walletAddress, string assetId, decimal transferAmount);
    }
}
