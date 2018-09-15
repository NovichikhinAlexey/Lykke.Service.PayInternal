using System;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.MarketProfile.Client.Models;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Lykke.Service.PayInternal.Services.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Lykke.Service.PayInternal.Services.Tests
{
    [TestClass]
    public class CalculationServiceTests
    {
        private Mock<IAssetsLocalCache> _assetsLocalCacheMock;
        private Mock<IAssetRatesService> _assetRatesService;

        private ICalculationService _service;

        private const int BtcAccuracy = 8;
        private const decimal MerchantPercent = 10;
        private const int MerchantPips = 20;

        [TestInitialize]
        public void TestInitialize()
        {
            _assetsLocalCacheMock = new Mock<IAssetsLocalCache>();
            _assetRatesService = new Mock<IAssetRatesService>();

            _service = new CalculationService(
                _assetsLocalCacheMock.Object,
                new LpMarkupSettings
                {
                    Percent = MerchantPercent,
                    Pips = MerchantPips
                },
                _assetRatesService.Object,
                Logs.EmptyLogFactory.Instance);
        }

        [TestMethod]
        public async Task CalculateBtcAmountFullfillmentAsync_PaidBelow()
        {
            const decimal plan = 10;

            const decimal fact = 9;

            _assetsLocalCacheMock.Setup(o => o.GetAssetByIdAsync(LykkeConstants.BitcoinAsset))
                .ReturnsAsync(new Asset {Accuracy = BtcAccuracy});

            var fullFillmentStatus = await _service.CalculateBtcAmountFullfillmentAsync(plan, fact);

            Assert.AreEqual(AmountFullFillmentStatus.Below, fullFillmentStatus);
        }

        [TestMethod]
        public async Task CalculateBtcAmountFullfillmentAsync_PaidAbove()
        {
            const decimal plan = 10;

            const decimal fact = 11;

            _assetsLocalCacheMock.Setup(o => o.GetAssetByIdAsync(LykkeConstants.BitcoinAsset))
                .ReturnsAsync(new Asset {Accuracy = BtcAccuracy});

            var fullFillmentStatus = await _service.CalculateBtcAmountFullfillmentAsync(plan, fact);

            Assert.AreEqual(AmountFullFillmentStatus.Above, fullFillmentStatus);
        }

        [TestMethod]
        public async Task CalculateBtcAmountFullfillmentAsync_PaidExact()
        {
            const decimal plan = 10;

            const decimal fact = 10;

            _assetsLocalCacheMock.Setup(o => o.GetAssetByIdAsync(LykkeConstants.BitcoinAsset))
                .ReturnsAsync(new Asset {Accuracy = BtcAccuracy});

            var fullFillmentStatus = await _service.CalculateBtcAmountFullfillmentAsync(plan, fact);

            Assert.AreEqual(AmountFullFillmentStatus.Exact, fullFillmentStatus);
        }


        [TestMethod]
        public async Task CalculateBtcAmountFullfillmentAsync_Accuracy_TooBig_Exact()
        {
            const decimal plan = (decimal) 10.000022229873549873459872;

            const decimal fact = (decimal) 10.000022226554828009123654;

            _assetsLocalCacheMock.Setup(o => o.GetAssetByIdAsync(LykkeConstants.BitcoinAsset))
                .ReturnsAsync(new Asset {Accuracy = BtcAccuracy});

            var fullFillmentStatus = await _service.CalculateBtcAmountFullfillmentAsync(plan, fact);

            Assert.AreEqual(AmountFullFillmentStatus.Exact, fullFillmentStatus);
        }

        [TestMethod]
        public async Task CalculateBtcAmountFullfillmentAsync_Accuracy_Less_Below()
        {
            const decimal plan = (decimal) 10.00002222;

            const decimal fact = (decimal) 10.00002221;

            _assetsLocalCacheMock.Setup(o => o.GetAssetByIdAsync(LykkeConstants.BitcoinAsset))
                .ReturnsAsync(new Asset {Accuracy = BtcAccuracy});

            var fullFillmentStatus = await _service.CalculateBtcAmountFullfillmentAsync(plan, fact);

            Assert.AreEqual(AmountFullFillmentStatus.Below, fullFillmentStatus);
        }

        [TestMethod]
        public async Task CalculateBtcAmountFullfillmentAsync_Accuracy_Less_Above()
        {
            const decimal plan = (decimal) 10.00002222;

            const decimal fact = (decimal) 10.00002225;

            _assetsLocalCacheMock.Setup(o => o.GetAssetByIdAsync(LykkeConstants.BitcoinAsset))
                .ReturnsAsync(new Asset {Accuracy = BtcAccuracy});

            var fullFillmentStatus = await _service.CalculateBtcAmountFullfillmentAsync(plan, fact);

            Assert.AreEqual(AmountFullFillmentStatus.Above, fullFillmentStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(NegativeValueException))]
        public async Task CalculateBtcAmountFullfillmentAsync_Plan_Negative()
        {
            const decimal plan = (decimal) -10.00002222;

            const decimal fact = (decimal) 10.00002222;

            await _service.CalculateBtcAmountFullfillmentAsync(plan, fact);
        }

        [TestMethod]
        [ExpectedException(typeof(NegativeValueException))]
        public async Task CalculateBtcAmountFullfillmentAsync_Fact_Negative()
        {
            const decimal plan = (decimal) 10.00002222;

            const decimal fact = (decimal) -10.00002222;

            await _service.CalculateBtcAmountFullfillmentAsync(plan, fact);
        }

        [TestMethod]
        public void CalculatePrice_CheckCorrectCalculation()
        {
            var assetPairRate = new AssetPairModel
            {
                AssetPair = "BTCCHF",
                BidPrice = 6838.57154,
                AskPrice = 6838.57154
            };

            IMarkup merchantMarkup = new Markup
            {
                Percent = 0,
                Pips = 0,
                DeltaSpread = 1.4m,
                FixedFee = 0
            };

            IRequestMarkup requestMarkup = new RequestMarkup
            {
                Pips = 9,
                Percent = 9,
                FixedFee = 13
            };

            var assetPair = new AssetPair
            {
                Id = "BTCCHF",
                Name = "BTC/CHF",
                Accuracy = 3,
                BaseAssetId = "BTC",
                QuotingAssetId = "CHF",
                InvertedAccuracy = 8,
                IsDisabled = false,
                Source = "BTCUSD",
                Source2 = "USDCHF"
            };

            decimal chfAmount = 10;

            var rate = _service.CalculatePrice((decimal) assetPairRate.AskPrice, (decimal) assetPairRate.BidPrice, assetPair.Accuracy,
                BtcAccuracy, requestMarkup.Percent, requestMarkup.Pips, PriceCalculationMethod.ByBid, merchantMarkup);

            var btcAmount = (chfAmount + requestMarkup.FixedFee + merchantMarkup.FixedFee) / rate;

            Assert.IsTrue(Math.Abs(btcAmount - (decimal) 0.00374839) < BtcAccuracy.GetMinValue());
        }

        [TestMethod]
        public void GetBasePriceByMethod_AskForBid_ReturnsBid()
        {
            const decimal bid = decimal.MinValue;
            const decimal ask = decimal.MaxValue;

            decimal calculated = _service.GetOriginalPriceByMethod(bid, ask, PriceCalculationMethod.ByBid);

            Assert.AreEqual(bid, calculated);
        }

        [TestMethod]
        public void GetOriginalPriceByMethod_AskForAsk_ReturnsAsk()
        {
            const decimal bid = decimal.MinValue;
            const decimal ask = decimal.MaxValue;

            decimal calculated = _service.GetOriginalPriceByMethod(bid, ask, PriceCalculationMethod.ByAsk);

            Assert.AreEqual(ask, calculated);
        }


        [TestMethod]
        public void GetSpread()
        {
            Assert.AreEqual(_service.GetSpread(100m, 1m), 1m);
            Assert.AreEqual(_service.GetSpread(100m, 100m), 100m);
            Assert.AreEqual(_service.GetSpread(100m, 120m), 120m);
            Assert.AreEqual(_service.GetSpread(0m, It.IsAny<decimal>()), 0m);
            Assert.AreEqual(_service.GetSpread(It.IsAny<decimal>(), 0m), 0m);
        }

        [TestMethod]
        [ExpectedException(typeof(NegativeValueException))]
        public void GetSpread_PercentLessZero_Exception()
        {
            _service.GetSpread(It.IsAny<decimal>(), -1m);
        }

        [TestMethod]
        public void GetPriceWithSpread_CalculateForAsk_CorrectValue()
        {
            decimal calculated = _service.GetPriceWithSpread(100m, 1m, PriceCalculationMethod.ByAsk);

            Assert.AreEqual(calculated, 101m);
        }

        [TestMethod]
        public void GetPriceWithSpread_CalculateForBid_CorrectValue()
        {
            decimal calculated = _service.GetPriceWithSpread(100m, 1m, PriceCalculationMethod.ByBid);

            Assert.AreEqual(calculated, 99m);
        }

        [TestMethod]
        public void GetMerchantFee_PositivePercent_ProvidedValueUsed()
        {
            const decimal originalValue = 100m;

            const decimal merchantPercent = 1m;

            const decimal merchantPercentFromSettings = MerchantPercent;

            decimal fee = _service.GetMerchantFee(originalValue, merchantPercent);

            Assert.AreNotEqual(merchantPercent, merchantPercentFromSettings);
            Assert.AreEqual(fee, originalValue * merchantPercent / 100);
        }

        [TestMethod]
        public void GetMerchantFee_NegativePercent_ValueFromSettingsUsed()
        {
            const decimal originalValue = 100m;

            const decimal merchantPercent = -1m;

            const decimal merchantPercentFromSettings = MerchantPercent;

            decimal fee = _service.GetMerchantFee(originalValue, merchantPercent);

            Assert.AreNotEqual(merchantPercent, merchantPercentFromSettings);
            Assert.AreEqual(fee, originalValue * merchantPercentFromSettings / 100);
        }

        [TestMethod]
        public void GetMerchantFee_ZeroPercent_ProvidedValueUsed()
        {
            const decimal originalValue = 100m;

            const decimal merchantPercent = 0m;

            const decimal merchantPercentFromSettings = MerchantPercent;

            decimal fee = _service.GetMerchantFee(originalValue, merchantPercent);

            Assert.AreNotEqual(merchantPercent, merchantPercentFromSettings);
            Assert.AreEqual(fee, 0m);
        }

        [TestMethod]
        public void GetMerchantPips_PositiveValue_ProvidedValueUsed()
        {
            const decimal merchantPips = 1m;

            var pips = _service.GetMerchantPips(merchantPips);

            Assert.AreEqual(merchantPips, pips);
        }

        [TestMethod]
        public void GetMerchantPips_NegativeValue_ValueFromSettingsUsed()
        {
            const decimal merchantPips = -1m;

            const decimal merchantPipsFromSettings = MerchantPips;

            var pips = _service.GetMerchantPips(merchantPips);

            Assert.AreNotEqual(merchantPips, merchantPipsFromSettings);
            Assert.AreEqual(pips, merchantPipsFromSettings);
        }

        [TestMethod]
        public void GetMerchantPips_ZeroValue_ProvidedValueUsed()
        {
            const decimal merchantPips = 0m;

            const decimal merchantPipsFromSettings = MerchantPips;

            var pips = _service.GetMerchantPips(merchantPips);

            Assert.AreNotEqual(merchantPips, merchantPipsFromSettings);
            Assert.AreEqual(pips, merchantPips);
        }

        [TestMethod]
        public void GetMarkupFeePerRequest()
        {
            Assert.AreEqual(_service.GetMarkupFeePerRequest(100m, 10m), 10m);
            Assert.AreEqual(_service.GetMarkupFeePerRequest(0m, It.IsAny<decimal>()), 0m);
            Assert.AreEqual(_service.GetMarkupFeePerRequest(It.IsAny<decimal>(), 0m), 0m);
            Assert.AreEqual(_service.GetMarkupFeePerRequest(100m, 120m), 120m);
        }

        [TestMethod]
        [ExpectedException(typeof(NegativeValueException))]
        public void GetMarkupFeePerRequest_PercentLessZero_Exception()
        {
            _service.GetMarkupFeePerRequest(It.IsAny<decimal>(), -1m);
        }

        [TestMethod]
        public void GetDelta()
        {
            const decimal spread = 100m;

            const decimal lpFee = 10m;

            const decimal markupFee = 20m;

            const decimal lpPips = 30m;

            const decimal markupPips = 40m;

            const int accuracy = BtcAccuracy;

            decimal calculated = _service.GetDelta(spread, lpFee, markupFee, lpPips, markupPips, accuracy);

            decimal delta = spread + lpFee + markupFee + (lpPips + markupPips) * accuracy.GetMinValue();

            Assert.AreEqual(calculated, delta);
            Assert.IsTrue(calculated > spread);
            Assert.IsTrue(calculated > lpFee);
            Assert.IsTrue(calculated > markupFee);
        }

        [TestMethod]
        public void GetPriceWithDelta_CalculateForAsk_CorrectValue()
        {
            decimal calculated = _service.GetPriceWithDelta(100m, 10m, PriceCalculationMethod.ByAsk);

            Assert.AreEqual(calculated, 110m);
        }

        [TestMethod]
        public void GetPriceWithDelta_CalculateForBid_CorrectValue()
        {
            decimal calculated = _service.GetPriceWithDelta(100m, 20m, PriceCalculationMethod.ByBid);

            Assert.AreEqual(calculated, 80m);
        }

        [TestMethod]
        public void GetPriceWithDelta_NegativeOriginalPriceByAsk_CorrectValue()
        {
            decimal calculated = _service.GetPriceWithDelta(-100m, 20m, PriceCalculationMethod.ByAsk);

            Assert.AreEqual(calculated, -80m);
        }

        [TestMethod]
        public void GetPriceWithDelta_NegativeOriginalPriceByBid_CorrectValue()
        {
            decimal calculated = _service.GetPriceWithDelta(-100m, 30m, PriceCalculationMethod.ByBid);

            Assert.AreEqual(calculated, -130m);
        }

        [TestMethod]
        public void GetPriceWithDelta_ZeroOriginalPrice_CorrectValue()
        {
            decimal calculated1 = _service.GetPriceWithDelta(0m, 30m, PriceCalculationMethod.ByBid);
            decimal calculated2 = _service.GetPriceWithDelta(0m, 40m, PriceCalculationMethod.ByAsk);

            Assert.AreEqual(calculated1, -30m);
            Assert.AreEqual(calculated2, 40m);
        }

        [TestMethod]
        public void GetPriceWithDelta_ZeroDelta_CorrectValue()
        {
            decimal calculated1 = _service.GetPriceWithDelta(100m, 0m, PriceCalculationMethod.ByBid);
            decimal calculated2 = _service.GetPriceWithDelta(200m, 0m, PriceCalculationMethod.ByAsk);

            Assert.AreEqual(calculated1, 100m);
            Assert.AreEqual(calculated2, 200m);
        }
    }
}
