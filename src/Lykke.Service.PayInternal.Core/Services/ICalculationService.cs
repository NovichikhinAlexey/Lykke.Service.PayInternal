using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Markup;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ICalculationService
    {
        Task<decimal> GetAmountAsync(string baseAssetId, string quotingAssetId, decimal amount, IRequestMarkup requestMarkup, IMarkup merchantMarkup);

        Task<decimal> GetRateAsync(string baseAssetId, string quotingAssetId, double markupPercent, int markupPips, IMarkup merchantMarkup);

        Task<AmountFullFillmentStatus> CalculateBtcAmountFullfillmentAsync(decimal plan, decimal fact);

        decimal CalculatePrice(double askPrice, double bidPrice, int pairAccuracy, int assetAccuracy, double markupPercent, int markupPips, PriceCalculationMethod priceValueType, IMarkup merchantMarkup);

        double GetOriginalPriceByMethod(double bid, double ask, PriceCalculationMethod method);

        double GetSpread(double originalPrice, decimal deltaSpreadPercent);

        double GetPriceWithSpread(double originalPrice, double spread, PriceCalculationMethod method);

        double GetMerchantFee(double originalPrice, decimal merchantPercent);

        double GetMerchantPips(double merchantPips);

        double GetMarkupFeePerRequest(double originalPrice, double markupPercentPerPerquest);

        decimal GetDelta(double spread, double lpFee, double markupFee, double lpPips, double markupPips, int accuracy);

        decimal GetPriceWithDelta(double originalPrice, decimal delta, PriceCalculationMethod method);
    }
}
