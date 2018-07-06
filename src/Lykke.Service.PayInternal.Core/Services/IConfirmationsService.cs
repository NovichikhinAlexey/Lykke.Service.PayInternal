using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Confirmations;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IConfirmationsService
    {
        Task ConfirmCashoutAsync(CashoutConfirmationCommand cmd);
    }
}
