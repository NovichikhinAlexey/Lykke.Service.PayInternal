using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Exchange;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface IExchangeApi
    {
        [Post("/api/exchange/Execute")]
        Task<ExchangeResponse> ExecuteAsync([Body] ExchangeRequest request);

        [Post("/api/exchange/PreExchange")]
        Task<ExchangeResponse> PreExchangeAsync([Body] PreExchangeRequest request);
    }
}
