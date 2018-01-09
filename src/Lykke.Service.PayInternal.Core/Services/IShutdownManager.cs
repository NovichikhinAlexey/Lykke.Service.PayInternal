using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}