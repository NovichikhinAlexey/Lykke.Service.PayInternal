using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.MarketProfile.Client;
using Lykke.Service.MarketProfile.Client.Models;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    public class CalculationService : ICalculationService
    {
        private readonly ILykkeMarketProfile _marketProfileServiceClient;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly LpMarkupSettings _lpMarkupSettings;
        private readonly ILog _log;

        public CalculationService(
            ILykkeMarketProfile marketProfileServiceClient,
            IAssetsLocalCache assetsLocalCache,
            LpMarkupSettings lpMarkupSettings,
            ILog log)
        {
            _marketProfileServiceClient = marketProfileServiceClient ??
                                          throw new ArgumentNullException(nameof(marketProfileServiceClient));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
            _lpMarkupSettings = lpMarkupSettings ?? throw new ArgumentNullException(nameof(lpMarkupSettings));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<decimal> GetAmountAsync(string assetPairId, decimal amount, IRequestMarkup requestMarkup, IMerchantMarkup merchantMarkup)
        {
            var rate = await GetRateAsync(assetPairId, requestMarkup.Percent, requestMarkup.Pips, merchantMarkup);

            await _log.WriteInfoAsync(nameof(CalculationService), nameof(GetAmountAsync), new
            {
                AssetPairId = assetPairId,
                Amount = amount,
                RequestMarkup = requestMarkup,
                MerchantMarkup = merchantMarkup,
                Rate = rate
            }.ToJson(), "Rate calculation");

            return (amount + (decimal) requestMarkup.FixedFee) / (decimal)rate;
        }

        public async Task<double> GetRateAsync(
            string assetPairId, 
            double markupPercent, 
            int markupPips, 
            IMerchantMarkup merchantMarkup)
        {
            var response = await _marketProfileServiceClient.ApiMarketProfileByPairCodeGetAsync(assetPairId);

            if (response is ErrorModel error)
            {
                throw new Exception(error.Message);
            }

            if (response is AssetPairModel assetPairRate)
            {
                var assetPair = await _assetsLocalCache.GetAssetPairByIdAsync(assetPairRate.AssetPair);

               return CalculatePrice(assetPairRate, assetPair.Accuracy, markupPercent, markupPips,
                    PriceCalculationMethod.ByBid, merchantMarkup);
            }

            throw new Exception("Unknown MarketProfile API response");
        }

        public async Task<AmountFullFillmentStatus> CalculateBtcAmountFullfillmentAsync(decimal plan, decimal fact)
        {
            if (plan < 0)
                throw new NegativeValueException(plan);

            if (fact < 0)
                throw new NegativeValueException(fact);

            var asset = await _assetsLocalCache.GetAssetByIdAsync(LykkeConstants.BitcoinAssetId);

            decimal diff = plan - fact;

            bool fullfilled = Math.Abs(diff) < asset.Accuracy.GetMinValue();

            if (fullfilled) 
                return AmountFullFillmentStatus.Exact;

            return diff > 0 ? AmountFullFillmentStatus.Below : AmountFullFillmentStatus.Above;
        }

        public double CalculatePrice(
            AssetPairModel assetPairRate, 
            int accuracy, 
            double markupPercent, 
            int markupPips, 
            PriceCalculationMethod priceValueType,
            IMerchantMarkup merchantMarkup)
        {
            _log.WriteInfoAsync(nameof(CalculationService), nameof(GetAmountAsync), assetPairRate.ToJson(),
                "Rate calculation").GetAwaiter().GetResult();

            double originalPrice = GetOriginalPriceByMethod(assetPairRate.BidPrice, assetPairRate.AskPrice, priceValueType);

            var origValue = originalPrice;

            var spread = GetSpread(originalPrice, merchantMarkup.DeltaSpread);

            originalPrice = GetPriceWithSpread(originalPrice, spread, priceValueType);

            double lpFee = GetMerchantFee(originalPrice, merchantMarkup.LpPercent);

            double lpPips = GetMerchantPips(merchantMarkup.LpPips);






            var delta = spread + lpFee + lpPips * 0.001;

            var fee = originalPrice * (markupPercent / 100);
            var pips =  Math.Pow(10, -1 * accuracy) * markupPips;

            delta += fee + pips;

            var result = origValue + (priceValueType == PriceCalculationMethod.ByAsk ? delta : -delta);

            var powRound = Math.Pow(10, -1 * accuracy) * (priceValueType == PriceCalculationMethod.ByAsk ? 0.49 : 0.5);

            result += priceValueType == PriceCalculationMethod.ByAsk ? powRound : -powRound;
            var res =  Math.Round(result, accuracy);
            int mult = (int)Math.Pow(10, accuracy);


            res = Math.Ceiling(res * mult) / mult;

            if (res < 0)
            {
                res = 0;
            }

            return res;
        }

        public double GetOriginalPriceByMethod(double bid, double ask, PriceCalculationMethod method)
        {
            switch (method)
            {
                case PriceCalculationMethod.ByAsk: return ask;
                case PriceCalculationMethod.ByBid: return bid;
                default: throw new UnexpectedPriceCalculationMethod(method);
            }
        }

        public double GetSpread(double originalPrice, double deltaSpreadPercent)
        {
            if (deltaSpreadPercent < 0)
                throw new NegativeValueException((decimal) deltaSpreadPercent);

            return originalPrice * deltaSpreadPercent / 100;
        }

        public double GetPriceWithSpread(double originalPrice, double spread, PriceCalculationMethod method)
        {
            switch (method)
            {
                case PriceCalculationMethod.ByBid: return originalPrice - spread;
                case PriceCalculationMethod.ByAsk: return originalPrice + spread;
                default: throw new UnexpectedPriceCalculationMethod(method);
            }
        }

        public double GetMerchantFee(double originalPrice, double merchantPercent)
        {
            var percent = merchantPercent < 0 ? _lpMarkupSettings.Percent : merchantPercent;

            return originalPrice * percent / 100;
        }

        public double GetMerchantPips(double merchantPips)
        {
            return merchantPips < 0 ? _lpMarkupSettings.Pips : merchantPips;
        }
    }
}
