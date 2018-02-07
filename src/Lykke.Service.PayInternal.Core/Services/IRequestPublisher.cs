using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IRequestPublisher<in T>
    {
        Task PublishAsync(T request);
    }
}
