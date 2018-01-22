using System;
using System.Threading.Tasks;
using Lykke.Service.MarketProfile.Client;
using Lykke.Service.MarketProfile.Client.Models;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    public class RatesCalculationService : IRatesCalculationService
    {
        internal enum PriceCalculationMethod
        {
            ByBid,
            ByAsk
        }

        private readonly ILykkeMarketProfile _marketProfileServiceClient;
        private readonly IAssetsLocalCache _assetsLocalCache;
        private readonly LpMarkupSettings _lpMarkupSettings;

        public RatesCalculationService(
            ILykkeMarketProfile marketProfileServiceClient,
            IAssetsLocalCache assetsLocalCache,
            LpMarkupSettings lpMarkupSettings)
        {
            _marketProfileServiceClient = marketProfileServiceClient ??
                                          throw new ArgumentNullException(nameof(marketProfileServiceClient));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
            _lpMarkupSettings = lpMarkupSettings ?? throw new ArgumentNullException(nameof(lpMarkupSettings));
        }

        public async Task<double> GetAmount(string assetPairId, double amount, IRequestMarkup requestMarkup, IMerchantMarkup merchantMarkup)
        {
            var rate = await GetRate(assetPairId, requestMarkup.Percent, requestMarkup.Pips, merchantMarkup);

            return (amount + requestMarkup.FixedFee) / rate;
        }

        public async Task<double> GetRate(
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


        //TODO: isolated legacy code, to be optimized and rewritten
        private double CalculatePrice(
            AssetPairModel assetPairRate, 
            int accuracy, 
            double markupPercent, 
            int markupPips, 
            PriceCalculationMethod priceValueType,
            IMerchantMarkup merchantMarkup)
        {
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
