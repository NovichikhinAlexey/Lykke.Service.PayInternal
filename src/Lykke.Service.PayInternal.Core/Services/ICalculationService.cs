using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Markup;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ICalculationService
    {
        Task<decimal> GetAmountAsync(string baseAssetId, string quotingAssetId, decimal amount, IRequestMarkup requestMarkup, IMarkup merchantMarkup);

        Task<decimal> GetRateAsync(string baseAssetId, string quotingAssetId, decimal markupPercent, int markupPips, IMarkup merchantMarkup);

        Task<AmountFullFillmentStatus> CalculateBtcAmountFullfillmentAsync(decimal plan, decimal fact);

        decimal CalculatePrice(decimal askPrice, decimal bidPrice, int pairAccuracy, int assetAccuracy, decimal markupPercent, int markupPips, PriceCalculationMethod priceValueType, IMarkup merchantMarkup);

        decimal GetOriginalPriceByMethod(decimal bid, decimal ask, PriceCalculationMethod method);

        decimal GetSpread(decimal originalPrice, decimal deltaSpreadPercent);

        decimal GetPriceWithSpread(decimal originalPrice, decimal spread, PriceCalculationMethod method);

        decimal GetMerchantFee(decimal originalPrice, decimal merchantPercent);

        decimal GetMerchantPips(decimal merchantPips);

        decimal GetMarkupFeePerRequest(decimal originalPrice, decimal markupPercentPerPerquest);

        decimal GetDelta(decimal spread, decimal lpFee, decimal markupFee, decimal lpPips, decimal markupPips, int accuracy);

        decimal GetPriceWithDelta(decimal originalPrice, decimal delta, PriceCalculationMethod method);
    }
}
