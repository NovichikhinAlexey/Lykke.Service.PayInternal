using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class TransactionsManager : ITransactionsManager
    {
        private readonly ITransactionsService _transactionsService;
        private readonly IPaymentRequestService _paymentRequestService;

        public TransactionsManager(
            ITransactionsService transactionsService, 
            IPaymentRequestService paymentRequestService)
        {
            _transactionsService = transactionsService;
            _paymentRequestService = paymentRequestService;
        }

        public async Task<IPaymentRequestTransaction> CreateTransactionAsync(ICreateTransactionCommand command)
        {
            IPaymentRequestTransaction transaction = await _transactionsService.CreateTransactionAsync(command);

            await _paymentRequestService.UpdateStatusAsync(command.WalletAddress);

            return transaction;
        }

        public async Task UpdateTransactionAsync(IUpdateTransactionCommand command)
        {
            await _transactionsService.UpdateAsync(command);

            string walletAddress = command.WalletAddress;

            if (string.IsNullOrEmpty(walletAddress))
            {
                IPaymentRequestTransaction tx =
                    await _transactionsService.GetByIdAsync(command.Blockchain, command.IdentityType, command.Identity);

                if (tx == null)
                    throw new TransactionNotFoundException(command.Blockchain, command.IdentityType, command.Identity);

                walletAddress = tx.WalletAddress;
            }

            await _paymentRequestService.UpdateStatusAsync(walletAddress);
        }
    }
}
