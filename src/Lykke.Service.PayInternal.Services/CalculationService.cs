using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.AssetPair;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    public class CalculationService : ICalculationService
    {
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly LpMarkupSettings _lpMarkupSettings;
        private readonly IAssetRatesService _assetRatesService;
        private readonly ILog _log;

        public CalculationService(
            [NotNull] IAssetsLocalCache assetsLocalCache,
            [NotNull] LpMarkupSettings lpMarkupSettings,
            [NotNull] IAssetRatesService assetRatesService,
            [NotNull] ILogFactory logFactory)
        {
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
            _lpMarkupSettings = lpMarkupSettings ?? throw new ArgumentNullException(nameof(lpMarkupSettings));
            _log = logFactory.CreateLog(this);
            _assetRatesService = assetRatesService ?? throw new ArgumentNullException(nameof(assetRatesService));
        }

        public async Task<decimal> GetAmountAsync(string baseAssetId, string quotingAssetId, decimal amount, IRequestMarkup requestMarkup,
            IMarkup merchantMarkup)
        {
            var rate = await GetRateAsync(baseAssetId, quotingAssetId, requestMarkup.Percent, requestMarkup.Pips, merchantMarkup);

            _log.Info("Rate calculation",
                $"Calculation details: {new {baseAssetId, quotingAssetId, amount, requestMarkup, merchantMarkup, rate}.ToJson()}");

            decimal result = (amount + requestMarkup.FixedFee + merchantMarkup.FixedFee) / rate;

            Asset baseAsset = await _assetsLocalCache.GetAssetByIdAsync(baseAssetId);

            decimal roundedResult = decimal.Round(result, baseAsset.Accuracy, MidpointRounding.AwayFromZero);

            return roundedResult;
        }

        public async Task<decimal> GetRateAsync(
            string baseAssetId, 
            string quotingAssetId,
            decimal markupPercent,
            int markupPips,
            IMarkup merchantMarkup)
        {
            decimal askPrice, bidPrice;

            AssetPair priceAssetPair = null, assetPair = null;

            if (!string.IsNullOrEmpty(merchantMarkup.PriceAssetPairId))
            {
                _log.Info($"Price asset pair will be used: {merchantMarkup.PriceAssetPairId}");

                priceAssetPair = await _assetsLocalCache.GetAssetPairByIdAsync(merchantMarkup.PriceAssetPairId);

                IAssetPairRate assetPairRate =
                    await _assetRatesService.GetCurrentRateAsync(priceAssetPair.BaseAssetId, priceAssetPair.QuotingAssetId);

                _log.Info($"Price method: {merchantMarkup.PriceMethod.ToString()}");

                switch (merchantMarkup.PriceMethod)
                {
                    case PriceMethod.None:
                    case PriceMethod.Direct:
                        askPrice = assetPairRate.AskPrice;
                        bidPrice = assetPairRate.BidPrice;
                        break;
                    case PriceMethod.Reverse:
                        askPrice = Math.Abs(assetPairRate.AskPrice) > 0
                            ? 1 / assetPairRate.AskPrice
                            : throw new MarketPriceZeroException("ask");
                        bidPrice = Math.Abs(assetPairRate.BidPrice) > 0
                            ? 1 / assetPairRate.BidPrice
                            : throw new MarketPriceZeroException("bid");
                        break;
                    default:
                        throw new UnexpectedAssetPairPriceMethodException(merchantMarkup.PriceMethod);
                }
            } 
            else 
            {
                assetPair = await _assetsLocalCache.GetAssetPairAsync(baseAssetId, quotingAssetId);

                try
                {
                    IAssetPairRate assetPairRate = await _assetRatesService.GetCurrentRateAsync(baseAssetId, quotingAssetId);

                    askPrice = assetPairRate.AskPrice;

                    bidPrice = assetPairRate.BidPrice;
                }
                catch (Exception)
                {
                    askPrice = bidPrice = 1;
                }
            }

            _log.Info($"Market rate that will be used for calculation, askPrice = {askPrice}, bidPrice = {bidPrice}");

            Asset baseAsset = await _assetsLocalCache.GetAssetByIdAsync(baseAssetId);

            int pairAccuracy = priceAssetPair?.Accuracy ?? assetPair?.Accuracy ?? baseAsset.Accuracy;

            return CalculatePrice(askPrice, bidPrice, pairAccuracy, baseAsset.Accuracy, markupPercent,
                markupPips, PriceCalculationMethod.ByBid, merchantMarkup);
        }

        public async Task<AmountFullFillmentStatus> CalculateBtcAmountFullfillmentAsync(decimal plan, decimal fact)
        {
            if (plan < 0)
                throw new NegativeValueException(plan);

            if (fact < 0)
                throw new NegativeValueException(fact);

            var asset = await _assetsLocalCache.GetAssetByIdAsync(LykkeConstants.BitcoinAsset);

            decimal diff = plan - fact;

            bool fullfilled = Math.Abs(diff) < asset.Accuracy.GetMinValue();

            if (fullfilled)
                return AmountFullFillmentStatus.Exact;

            return diff > 0 ? AmountFullFillmentStatus.Below : AmountFullFillmentStatus.Above;
        }

        public decimal CalculatePrice(
            decimal askPrice, 
            decimal bidPrice,
            int pairAccuracy,
            int assetAccuracy,
            decimal markupPercent,
            int markupPips,
            PriceCalculationMethod priceValueType,
            IMarkup merchantMarkup)
        {
            _log.Info($"Rate calculation, askPrice = {askPrice}, bidPrice = {bidPrice}");

            decimal originalPrice =
                GetOriginalPriceByMethod(bidPrice, askPrice, priceValueType);

            decimal spread = GetSpread(originalPrice, merchantMarkup.DeltaSpread);

            decimal priceWithSpread = GetPriceWithSpread(originalPrice, spread, priceValueType);

            decimal lpFee = GetMerchantFee(priceWithSpread, merchantMarkup.Percent);

            decimal lpPips = GetMerchantPips(merchantMarkup.Pips);

            decimal fee = GetMarkupFeePerRequest(priceWithSpread, markupPercent);

            decimal delta = GetDelta(spread, lpFee, fee, lpPips, markupPips, pairAccuracy);

            decimal result = GetPriceWithDelta(originalPrice, delta, priceValueType);

            return GetRoundedPrice(result, pairAccuracy, assetAccuracy, priceValueType);
        }

        public decimal GetOriginalPriceByMethod(decimal bid, decimal ask, PriceCalculationMethod method)
        {
            switch (method)
            {
                case PriceCalculationMethod.ByAsk: return ask;
                case PriceCalculationMethod.ByBid: return bid;
                default: throw new UnexpectedPriceCalculationMethodException(method);
            }
        }

        public decimal GetSpread(decimal originalPrice, decimal deltaSpreadPercent)
        {
            if (deltaSpreadPercent < 0)
                throw new NegativeValueException(deltaSpreadPercent);

            return originalPrice * deltaSpreadPercent / 100;
        }

        public decimal GetPriceWithSpread(decimal originalPrice, decimal spread, PriceCalculationMethod method)
        {
            switch (method)
            {
                case PriceCalculationMethod.ByBid: return originalPrice - spread;
                case PriceCalculationMethod.ByAsk: return originalPrice + spread;
                default: throw new UnexpectedPriceCalculationMethodException(method);
            }
        }

        public decimal GetMerchantFee(decimal originalPrice, decimal merchantPercent)
        {
            var percent = merchantPercent < 0 ? _lpMarkupSettings.Percent : merchantPercent;

            return originalPrice * percent / 100;
        }

        public decimal GetMerchantPips(decimal merchantPips)
        {
            return merchantPips < 0 ? _lpMarkupSettings.Pips : merchantPips;
        }

        public decimal GetMarkupFeePerRequest(decimal originalPrice, decimal markupPercentPerPerquest)
        {
            if (markupPercentPerPerquest < 0)
                throw new NegativeValueException(markupPercentPerPerquest);

            return originalPrice * markupPercentPerPerquest / 100;
        }

        public decimal GetDelta(
            decimal spread,
            decimal lpFee,
            decimal markupFee,
            decimal lpPips,
            decimal markupPips,
            int accuracy)
        {
            decimal totalFee = lpFee + markupFee;

            decimal totalPips = lpPips + markupPips;

            return
                spread +
                totalFee +
                totalPips * accuracy.GetMinValue();
        }

        public decimal GetPriceWithDelta(decimal originalPrice, decimal delta, PriceCalculationMethod method)
        {
            switch (method)
            {
                case PriceCalculationMethod.ByBid: return originalPrice - delta;
                case PriceCalculationMethod.ByAsk: return originalPrice + delta;
                default: throw new UnexpectedPriceCalculationMethodException(method);
            }
        }

        public decimal GetRoundedPrice(decimal originalPrice, int pairAccuracy, int assetAccuracy,
            PriceCalculationMethod method)
        {
            decimal result;

            switch (method)
            {
                case PriceCalculationMethod.ByBid:
                    result = originalPrice - pairAccuracy.GetMinValue() * (decimal) 0.5;
                    break;
                case PriceCalculationMethod.ByAsk:
                    result = originalPrice + pairAccuracy.GetMinValue() * (decimal) 0.49;
                    break;
                default: throw new UnexpectedPriceCalculationMethodException(method);
            }

            decimal rounded = Math.Round(result, assetAccuracy);

            int mult = (int) Math.Pow(10, assetAccuracy);

            decimal ceiled = Math.Ceiling(rounded * mult) / mult;

            return ceiled < 0 ? 0 : ceiled;
        }
    }
}
