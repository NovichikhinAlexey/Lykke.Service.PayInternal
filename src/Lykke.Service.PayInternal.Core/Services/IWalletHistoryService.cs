using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.History;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IWalletHistoryService
    {
        Task<string> PublishCashInAsync(IWalletHistoryCommand cmd);

        Task<string> PublishOutgoingExchangeAsync(IWalletHistoryCommand cmd);

        Task<string> PublishIncomingExchangeAsync(IWalletHistoryCommand cmd);

        Task<string> PublishCashoutAsync(IWalletHistoryCashoutCommand cmd);

        Task SetTxHashAsync(string id, string hash);

        Task RemoveAsync(string id);
    }
}
