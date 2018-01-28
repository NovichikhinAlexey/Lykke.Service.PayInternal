using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Wallet;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IWalletEventsPublisher
    {
        Task PublishAsync(IWallet wallet);
    }
}
