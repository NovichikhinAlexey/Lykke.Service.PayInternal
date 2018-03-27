using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ITransferService
    {
        Task<TransferResult> ExecuteAsync(TransferCommand transfer);

        Task<Transfer> GetAsync(string id);
    }
}
