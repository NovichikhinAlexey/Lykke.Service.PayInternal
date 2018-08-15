using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ITransactionsManager : IEthereumTransactionsManager
    {
        Task<IPaymentRequestTransaction> CreateTransactionAsync(ICreateTransactionCommand command);
        Task UpdateTransactionAsync(IUpdateTransactionCommand cmd);
    }
}
