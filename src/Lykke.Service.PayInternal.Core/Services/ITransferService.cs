using Lykke.Service.PayInternal.Core.Domain.Transfer;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ITransferService
    {
        Task<string> ExecuteAsync(BtcTransfer transfer);

        Task<MultipartTransferResponse> ExecuteMultipartTransferAsync(IMultipartTransfer multipartTransfer);
    }
}
