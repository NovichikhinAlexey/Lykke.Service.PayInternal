using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Cashout;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    internal interface ICashoutApi
    {
        [Post("/api/cashout/Execute")]
        Task<CashoutResponse> ExecuteAsync([Body] CashoutRequest request);
    }
}
