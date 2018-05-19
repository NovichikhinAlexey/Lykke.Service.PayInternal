using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ITransactionsManager
    {
        Task<IPaymentRequestTransaction> CreateTransactionAsync(ICreateTransactionCommand command);

        Task<IPaymentRequestTransaction> CreateLykkeTransactionAsync(ICreateLykkeTransactionCommand command);

        Task UpdateTransactionAsync(IUpdateTransactionCommand command);
    }
}
