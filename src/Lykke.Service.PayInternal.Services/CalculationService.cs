﻿using System;
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

        public async Task<decimal> GetAmountAsync(string assetPairId, decimal amount, IRequestMarkup requestMarkup,
            IMerchantMarkup merchantMarkup)
        {
            var rate = await GetRateAsync(assetPairId, requestMarkup.Percent, requestMarkup.Pips, merchantMarkup);

            await _log.WriteInfoAsync(nameof(CalculationService), nameof(GetAmountAsync), new
            {
                AssetPairId = assetPairId,
                Amount = amount,
                RequestMarkup = requestMarkup,
                MerchantMarkup = merchantMarkup,
                CalculatedRate = rate
            }.ToJson(), "Rate calculation");

            decimal result = (amount + (decimal) requestMarkup.FixedFee + (decimal) merchantMarkup.LpFixedFee) / rate;

            var assetPair = await _assetsLocalCache.GetAssetPairByIdAsync(assetPairId);

            var baseAsset = await _assetsLocalCache.GetAssetByIdAsync(assetPair.BaseAssetId);

            decimal roundedResult = decimal.Round(result, baseAsset.Accuracy, MidpointRounding.AwayFromZero);

            return roundedResult;
        }

        public async Task<decimal> GetRateAsync(
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

                var baseAsset = await _assetsLocalCache.GetAssetByIdAsync(assetPair.BaseAssetId);

                return CalculatePrice(assetPairRate, assetPair.Accuracy, baseAsset.Accuracy, markupPercent, markupPips,
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

        public decimal CalculatePrice(
            AssetPairModel assetPairRate,
            int pairAccuracy,
            int assetAccuracy,
            double markupPercent,
            int markupPips,
            PriceCalculationMethod priceValueType,
            IMerchantMarkup merchantMarkup)
        {
            _log.WriteInfoAsync(nameof(CalculationService), nameof(CalculatePrice), assetPairRate.ToJson(),
                "Rate calculation").GetAwaiter().GetResult();

            double originalPrice =
                GetOriginalPriceByMethod(assetPairRate.BidPrice, assetPairRate.AskPrice, priceValueType);

            double spread = GetSpread(originalPrice, merchantMarkup.DeltaSpread);

            double priceWithSpread = GetPriceWithSpread(originalPrice, spread, priceValueType);

            double lpFee = GetMerchantFee(priceWithSpread, merchantMarkup.LpPercent);

            double lpPips = GetMerchantPips(merchantMarkup.LpPips);

            double fee = GetMarkupFeePerRequest(priceWithSpread, markupPercent);

            decimal delta = GetDelta(spread, lpFee, fee, lpPips, markupPips, pairAccuracy);

            decimal result = GetPriceWithDelta(originalPrice, delta, priceValueType);

            return GetRoundedPrice(result, pairAccuracy, assetAccuracy, priceValueType);
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

        public double GetMarkupFeePerRequest(double originalPrice, double markupPercentPerPerquest)
        {
            if (markupPercentPerPerquest < 0)
                throw new NegativeValueException((decimal) markupPercentPerPerquest);

            return originalPrice * markupPercentPerPerquest / 100;
        }

        public decimal GetDelta(
            double spread,
            double lpFee,
            double markupFee,
            double lpPips,
            double markupPips,
            int accuracy)
        {
            double totalFee = lpFee + markupFee;

            double totalPips = lpPips + markupPips;

            return
                (decimal) spread +
                (decimal) totalFee +
                (decimal) totalPips * accuracy.GetMinValue();
        }

        public decimal GetPriceWithDelta(double originalPrice, decimal delta, PriceCalculationMethod method)
        {
            switch (method)
            {
                case PriceCalculationMethod.ByBid: return (decimal) originalPrice - delta;
                case PriceCalculationMethod.ByAsk: return (decimal) originalPrice + delta;
                default: throw new UnexpectedPriceCalculationMethod(method);
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
                default: throw new UnexpectedPriceCalculationMethod(method);
            }

            decimal rounded = Math.Round(result, assetAccuracy);

            int mult = (int) Math.Pow(10, assetAccuracy);

            decimal ceiled = Math.Ceiling(rounded * mult) / mult;

            return ceiled < 0 ? 0 : ceiled;
        }
    }
}
