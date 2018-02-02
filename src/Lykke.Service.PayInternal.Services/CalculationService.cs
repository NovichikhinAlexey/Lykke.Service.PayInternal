using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.MarketProfile.Client;
using Lykke.Service.MarketProfile.Client.Models;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    public class CalculationService : ICalculationService
    {
        internal enum PriceCalculationMethod
        {
            ByBid,
            ByAsk
        }

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
            var asset = await _assetsLocalCache.GetAssetByIdAsync(LykkeConstants.BitcoinAssetId);

            decimal diff = plan - fact;

            bool fullfilled = Math.Abs(diff) <= asset.Accuracy.GetMinValue();

            if (fullfilled) 
                return AmountFullFillmentStatus.Exact;

            return diff > 0 ? AmountFullFillmentStatus.Below : AmountFullFillmentStatus.Above;
        }

        //TODO: isolated legacy code, to be optimized and rewritten
        private double CalculatePrice(
            AssetPairModel assetPairRate, 
            int accuracy, 
            double markupPercent, 
            int markupPips, 
            PriceCalculationMethod priceValueType,
            IMerchantMarkup merchantMarkup)
        {
            _log.WriteInfoAsync(nameof(CalculationService), nameof(GetAmountAsync), assetPairRate.ToJson(),
                "Rate calculation").GetAwaiter().GetResult();

            double value = priceValueType == PriceCalculationMethod.ByBid ? assetPairRate.BidPrice : assetPairRate.AskPrice;

            var origValue = value;
            var spread = value * (merchantMarkup.DeltaSpread/100);
            value = priceValueType == PriceCalculationMethod.ByAsk ? (value + spread) : (value - spread);
            double lpFee = value * (merchantMarkup.LpPercent < 0 ? _lpMarkupSettings.Percent/100 : merchantMarkup.LpPercent / 100);
            double lpPips = Math.Pow(10, -1 * accuracy) * merchantMarkup.LpPips < 0 ? _lpMarkupSettings.Pips : merchantMarkup.LpPips;

            var delta = spread + lpFee + lpPips;

            var fee = value * (markupPercent / 100);
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
    }
}
