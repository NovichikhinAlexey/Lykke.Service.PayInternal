using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.History;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IWalletHistoryService
    {
        Task PublishCashInAsync(IWalletHistoryCommand cmd);

        Task PublishOutgoingExchangeAsync(IWalletHistoryCommand cmd);

        Task PublishIncomingExchangeAsync(IWalletHistoryCommand cmd);

        Task PublishCashoutAsync(IWalletHistoryCashoutCommand cmd);
    }
}
