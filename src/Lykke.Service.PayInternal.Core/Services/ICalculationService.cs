using System.Threading.Tasks;
using Lykke.Service.MarketProfile.Client.Models;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Merchant;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ICalculationService
    {
        Task<decimal> GetAmountAsync(string assetPairId, decimal amount, IRequestMarkup requestMarkup, IMerchantMarkup merchantMarkup);

        Task<double> GetRateAsync(string assetPairId, double markupPercent, int markupPips, IMerchantMarkup merchantMarkup);

        Task<AmountFullFillmentStatus> CalculateBtcAmountFullfillmentAsync(decimal plan, decimal fact);

        double CalculatePrice(AssetPairModel assetPairRate, int accuracy, double markupPercent, int markupPips,
            PriceCalculationMethod priceValueType, IMerchantMarkup merchantMarkup);

        double GetOriginalPriceByMethod(double bid, double ask, PriceCalculationMethod method);

        double GetSpread(double originalPrice, double deltaSpreadPercent);

        double GetPriceWithSpread(double originalPrice, double spread, PriceCalculationMethod method);

        double GetMerchantFee(double originalPrice, double merchantPercent);

        double GetMerchantPips(double merchantPips);
    }
}
