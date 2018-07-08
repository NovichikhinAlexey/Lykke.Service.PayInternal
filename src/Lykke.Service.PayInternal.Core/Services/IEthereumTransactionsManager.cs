using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transaction.Ethereum.Common;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IEthereumTransactionsManager
    {
        Task RegisterInboundAsync(RegisterInTxCommand cmd);
        Task UpdateOutgoingAsync(UpdateOutTxCommand cmd);
        Task CompleteOutgoingAsync(CompleteOutTxCommand cmd);
        Task FailOutgoingAsync(NotEnoughFundsOutTxCommand cmd);
        Task FailOutgoingAsync(FailOutTxCommand cmd);
    }
}
