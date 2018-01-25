using System.Threading.Tasks;
using Autofac;
using Common;
using Lykke.Service.PayInternal.Contract;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ITransactionUpdatesPublisher : IStartable, IStopable
    {
        Task PublishAsync(TransactionUpdateMessage message);
    }
}
