using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Cashout;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ICashoutService
    {
        Task<CashoutResult> ExecuteAsync(CashoutCommand cmd);
    }
}
