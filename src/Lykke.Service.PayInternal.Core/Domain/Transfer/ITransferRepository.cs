using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public interface ITransferRepository
    {
        Task<ITransfer> AddAsync(ITransfer transfer);
    }
}
