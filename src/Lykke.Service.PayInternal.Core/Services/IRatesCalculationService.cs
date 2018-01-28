using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Merchant;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IRatesCalculationService
    {
        Task<decimal> GetAmount(string assetPairId, decimal amount, IRequestMarkup requestMarkup, IMerchantMarkup merchantMarkup);

        Task<double> GetRate(string assetPairId, double markupPercent, int markupPips, IMerchantMarkup merchantMarkup);
    }
}
