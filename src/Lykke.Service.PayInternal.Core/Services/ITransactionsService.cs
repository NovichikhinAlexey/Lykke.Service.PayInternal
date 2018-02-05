using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ITransactionsService
    {
        Task<IEnumerable<IBlockchainTransaction>> GetAsync(string walletAddress);
        Task Create(ICreateTransaction request);
        Task Update(IUpdateTransaction request);
    }
}
