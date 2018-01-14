using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Merchant;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IRatesCalculationService
    {
        Task<double> GetRate(string assetPairId, double markupPercent, int markupPips, IMerchantMarkup merchantMarkup);
    }
}
