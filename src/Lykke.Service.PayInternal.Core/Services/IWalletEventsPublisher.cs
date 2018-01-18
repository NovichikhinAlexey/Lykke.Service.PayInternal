using System.Threading.Tasks;
using Autofac;
using Common;
using Lykke.Service.PayInternal.Contract;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IWalletEventsPublisher : IStartable, IStopable
    {
        Task PublishAsync(NewWalletMessage message);
    }
}
