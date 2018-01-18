using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface IBlockchainTransactionRepository
    {
        Task SaveAsync(IBlockchainTransaction tx);
    }
}
