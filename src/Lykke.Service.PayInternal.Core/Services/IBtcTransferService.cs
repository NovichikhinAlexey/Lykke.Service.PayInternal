using Lykke.Service.PayInternal.Core.Domain.Transfer;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IBtcTransferService
    {
        Task<string> ExecuteAsync(BtcTransfer transfer);

        Task<MultipartTransferResponse> ExecuteMultipartTransferAsync(IMultipartTransfer multipartTransfer, TransactionType transactionsType);
    }
}
