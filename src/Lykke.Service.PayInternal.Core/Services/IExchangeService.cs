using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Exchange;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IExchangeService
    {
        Task<ExchangeResult> ExecuteAsync(ExchangeCommand cmd);
        Task<ExchangeResult> PreExchangeAsync(PreExchangeCommand cmd);
    }
}
