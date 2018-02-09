using System.Threading.Tasks;
using Lykke.Service.MarketProfile.Client.Models;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Merchant;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ICalculationService
    {
        Task<decimal> GetAmountAsync(string assetPairId, decimal amount, IRequestMarkup requestMarkup, IMerchantMarkup merchantMarkup);

        Task<decimal> GetRateAsync(string assetPairId, double markupPercent, int markupPips, IMerchantMarkup merchantMarkup);

        Task<AmountFullFillmentStatus> CalculateBtcAmountFullfillmentAsync(decimal plan, decimal fact);

        decimal CalculatePrice(AssetPairModel assetPairRate, int pairAccuracy, int assetAccuracy, double markupPercent,
            int markupPips, PriceCalculationMethod priceValueType, IMerchantMarkup merchantMarkup);

        double GetOriginalPriceByMethod(double bid, double ask, PriceCalculationMethod method);

        double GetSpread(double originalPrice, double deltaSpreadPercent);

        double GetPriceWithSpread(double originalPrice, double spread, PriceCalculationMethod method);

        double GetMerchantFee(double originalPrice, double merchantPercent);

        double GetMerchantPips(double merchantPips);

        double GetMarkupFeePerRequest(double originalPrice, double markupPercentPerPerquest);

        decimal GetDelta(double spread, double lpFee, double markupFee, double lpPips, double markupPips, int accuracy);

        decimal GetPriceWithDelta(double originalPrice, decimal delta, PriceCalculationMethod method);
    }
}
