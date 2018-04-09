using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ITransactionsService
    {
        Task<IEnumerable<IPaymentRequestTransaction>> GetAsync(string walletAddress);

        Task<IEnumerable<IPaymentRequestTransaction>> GetByPaymentRequestAsync(string paymentRequestId);

        Task<IEnumerable<IPaymentRequestTransaction>> GetConfirmedAsync(string walletAddress);

        Task<IEnumerable<IPaymentRequestTransaction>> GetAllMonitoredAsync();

        Task<IPaymentRequestTransaction> CreateTransactionAsync(ICreateTransactionCommand request);

        Task UpdateAsync(IUpdateTransactionCommand request);
        Task<IReadOnlyList<IPaymentRequestTransaction>> GetTransactionsByPaymentRequestAsync(string paymentRequestId);
    }
}
