using Lykke.Service.PayInternal.Core.Domain.Transaction;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ITransactionPublisher
    {
        Task PublishAsync(IBlockchainTransaction transaction);
    }
}
