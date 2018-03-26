using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public interface ITransferRepository
    {
        Task<Transfer> AddAsync(ITransfer transfer);

        Task<Transfer> GetAsync(string id);
    }
}
