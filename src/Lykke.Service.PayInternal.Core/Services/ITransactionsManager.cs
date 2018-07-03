using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ITransactionsManager
    {
        Task<IPaymentRequestTransaction> CreateTransactionAsync(ICreateTransactionCommand command);

        Task RegisterEthInboundTxAsync(RegisterEthInboundTxCommand cmd);

        Task UpdateEthOutgoingTxAsync(UpdateEthOutgoingTxCommand cmd);

        Task UpdateTransactionAsync(IUpdateTransactionCommand cmd);

        Task CompleteEthOutgoingTxAsync(CompleteEthOutgoingTxCommand cmd);

        Task FailEthOutgoingTxAsync(NotEnoughFundsEthOutgoingTxCommand cmd);
    }
}
