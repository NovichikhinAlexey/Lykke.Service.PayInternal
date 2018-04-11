using System;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IWalletEventsPublisher
    {
        Task PublishAsync(string walletAddress, BlockchainType blockchain, DateTime dueDate);
    }
}
