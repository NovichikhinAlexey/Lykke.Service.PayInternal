using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.History;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IWalletHistoryService
    {
        Task PublishCashIn(IWalletHistoryCommand cmd);

        Task PublishOutgoingExchange(IWalletHistoryCommand cmd);

        Task PublishIncomingExchange(IWalletHistoryCommand cmd);
    }
}
