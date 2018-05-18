using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.MarketProfile.Client;
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
        private Mock<ILykkeMarketProfile> _marketProfileServiceMock;
        private Mock<IAssetsLocalCache> _assetsLocalCacheMock;
        private Mock<ILog> _logMock;

        private ICalculationService _service;

        private const int BtcAccuracy = 8;
        private const double MerchantPercent = 10;
        private const int MerchantPips = 20;

        [TestInitialize]
        public void TestInitialize()
        {
            _assetsLocalCacheMock = new Mock<IAssetsLocalCache>();
            _marketProfileServiceMock = new Mock<ILykkeMarketProfile>();
            _logMock = new Mock<ILog>();

            _service = new CalculationService(
                _marketProfileServiceMock.Object,
                _assetsLocalCacheMock.Object,
                new LpMarkupSettings
                {
                    Percent = MerchantPercent,
                    Pips = MerchantPips
                },
                _logMock.Object);
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

            _logMock.Setup(o => o.WriteInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>())).Verifiable();

            var rate = _service.CalculatePrice(assetPairRate.AskPrice, assetPairRate.BidPrice, assetPair.Accuracy,
                BtcAccuracy, requestMarkup.Percent, requestMarkup.Pips, PriceCalculationMethod.ByBid, merchantMarkup);

            var btcAmount = (chfAmount + (decimal) requestMarkup.FixedFee + merchantMarkup.FixedFee) / rate;

            Assert.IsTrue(Math.Abs(btcAmount - (decimal) 0.00374839) < BtcAccuracy.GetMinValue());
        }

        [TestMethod]
        public void GetBasePriceByMethod_AskForBid_ReturnsBid()
        {
            const double bid = double.MinValue;
            const double ask = double.MaxValue;

            double calculcated = _service.GetOriginalPriceByMethod(bid, ask, PriceCalculationMethod.ByBid);

            Assert.AreEqual(bid, calculcated);
        }

        [TestMethod]
        public void GetOriginalPriceByMethod_AskForAsk_ReturnsAsk()
        {
            const double bid = double.MinValue;
            const double ask = double.MaxValue;

            double calculcated = _service.GetOriginalPriceByMethod(bid, ask, PriceCalculationMethod.ByAsk);

            Assert.AreEqual(ask, calculcated);
        }


        [TestMethod]
        public void GetSpread()
        {
            Assert.AreEqual(_service.GetSpread(100d, 1m), 1d);
            Assert.AreEqual(_service.GetSpread(100d, 100m), 100d);
            Assert.AreEqual(_service.GetSpread(100d, 120m), 120d);
            Assert.AreEqual(_service.GetSpread(0d, It.IsAny<decimal>()), 0d);
            Assert.AreEqual(_service.GetSpread(It.IsAny<double>(), 0m), 0d);
        }

        [TestMethod]
        [ExpectedException(typeof(NegativeValueException))]
        public void GetSpread_PercentLessZero_Exception()
        {
            _service.GetSpread(It.IsAny<double>(), -1m);
        }

        [TestMethod]
        public void GetPriceWithSpread_CalculateForAsk_CorrectValue()
        {
            double calculated = _service.GetPriceWithSpread(100d, 1d, PriceCalculationMethod.ByAsk);

            Assert.AreEqual(calculated, 101d);
        }

        [TestMethod]
        public void GetPriceWithSpread_CalculateForBid_CorrectValue()
        {
            double calculated = _service.GetPriceWithSpread(100d, 1d, PriceCalculationMethod.ByBid);

            Assert.AreEqual(calculated, 99d);
        }

        [TestMethod]
        public void GetMerchantFee_PositivePercent_ProvidedValueUsed()
        {
            const double originalValue = 100d;

            const decimal merchantPercent = 1m;

            const double merchantPercentFromSettings = MerchantPercent;

            double fee = _service.GetMerchantFee(originalValue, merchantPercent);

            Assert.AreNotEqual(merchantPercent, merchantPercentFromSettings);
            Assert.AreEqual(fee, originalValue * (double) merchantPercent / 100);
        }

        [TestMethod]
        public void GetMerchantFee_NegativePercent_ValueFromSettingsUsed()
        {
            const double originalValue = 100d;

            const decimal merchantPercent = -1m;

            const double merchantPercentFromSettings = MerchantPercent;

            double fee = _service.GetMerchantFee(originalValue, merchantPercent);

            Assert.AreNotEqual(merchantPercent, merchantPercentFromSettings);
            Assert.AreEqual(fee, originalValue * merchantPercentFromSettings / 100);
        }

        [TestMethod]
        public void GetMerchantFee_ZeroPercent_ProvidedValueUsed()
        {
            const double originalValue = 100d;

            const decimal merchantPercent = 0m;

            const double merchantPercentFromSettings = MerchantPercent;

            double fee = _service.GetMerchantFee(originalValue, merchantPercent);

            Assert.AreNotEqual(merchantPercent, merchantPercentFromSettings);
            Assert.AreEqual(fee, 0d);
        }

        [TestMethod]
        public void GetMerchantPips_PositiveValue_ProvidedValueUsed()
        {
            const double merchantPips = 1d;

            var pips = _service.GetMerchantPips(merchantPips);

            Assert.AreEqual(merchantPips, pips);
        }

        [TestMethod]
        public void GetMerchantPips_NegativeValue_ValueFromSettingsUsed()
        {
            const double merchantPips = -1d;

            const double merchantPipsFromSettings = MerchantPips;

            var pips = _service.GetMerchantPips(merchantPips);

            Assert.AreNotEqual(merchantPips, merchantPipsFromSettings);
            Assert.AreEqual(pips, merchantPipsFromSettings);
        }

        [TestMethod]
        public void GetMerchantPips_ZeroValue_ProvidedValueUsed()
        {
            const double merchantPips = 0d;

            const double merchantPipsFromSettings = MerchantPips;

            var pips = _service.GetMerchantPips(merchantPips);

            Assert.AreNotEqual(merchantPips, merchantPipsFromSettings);
            Assert.AreEqual(pips, merchantPips);
        }

        [TestMethod]
        public void GetMarkupFeePerRequest()
        {
            Assert.AreEqual(_service.GetMarkupFeePerRequest(100d, 10d), 10d);
            Assert.AreEqual(_service.GetMarkupFeePerRequest(0d, It.IsAny<double>()), 0d);
            Assert.AreEqual(_service.GetMarkupFeePerRequest(It.IsAny<double>(), 0d), 0d);
            Assert.AreEqual(_service.GetMarkupFeePerRequest(100d, 120d), 120d);
        }

        [TestMethod]
        [ExpectedException(typeof(NegativeValueException))]
        public void GetMarkupFeePerRequest_PercentLessZero_Exception()
        {
            _service.GetMarkupFeePerRequest(It.IsAny<double>(), -1d);
        }

        [TestMethod]
        public void GetDelta()
        {
            const double spread = 100d;

            const double lpFee = 10d;

            const double markupFee = 20d;

            const double lpPips = 30d;

            const double markupPips = 40d;

            const int accuracy = BtcAccuracy;

            decimal calculated = _service.GetDelta(spread, lpFee, markupFee, lpPips, markupPips, accuracy);

            decimal delta = (decimal) (spread + lpFee + markupFee) + (decimal) (lpPips + markupPips) * accuracy.GetMinValue();

            Assert.AreEqual(calculated, delta);
            Assert.IsTrue(calculated > (decimal) spread);
            Assert.IsTrue(calculated > (decimal) lpFee);
            Assert.IsTrue(calculated > (decimal) markupFee);
        }

        [TestMethod]
        public void GetPriceWithDelta_CalculateForAsk_CorrectValue()
        {
            decimal calculated = _service.GetPriceWithDelta(100d, 10, PriceCalculationMethod.ByAsk);

            Assert.AreEqual(calculated, 110);
        }

        [TestMethod]
        public void GetPriceWithDelta_CalculateForBid_CorrectValue()
        {
            decimal calculated = _service.GetPriceWithDelta(100d, 20, PriceCalculationMethod.ByBid);

            Assert.AreEqual(calculated, 80);
        }

        [TestMethod]
        public void GetPriceWithDelta_NegativeOriginalPriceByAsk_CorrectValue()
        {
            decimal calculated = _service.GetPriceWithDelta(-100d, 20, PriceCalculationMethod.ByAsk);

            Assert.AreEqual(calculated, -80);
        }

        [TestMethod]
        public void GetPriceWithDelta_NegativeOriginalPriceByBid_CorrectValue()
        {
            decimal calculated = _service.GetPriceWithDelta(-100d, 30, PriceCalculationMethod.ByBid);

            Assert.AreEqual(calculated, -130);
        }

        [TestMethod]
        public void GetPriceWithDelta_ZeroOriginalPrice_CorrectValue()
        {
            decimal calculated1 = _service.GetPriceWithDelta(0, 30, PriceCalculationMethod.ByBid);
            decimal calculated2 = _service.GetPriceWithDelta(0, 40, PriceCalculationMethod.ByAsk);

            Assert.AreEqual(calculated1, -30);
            Assert.AreEqual(calculated2, 40);
        }

        [TestMethod]
        public void GetPriceWithDelta_ZeroDelta_CorrectValue()
        {
            decimal calculated1 = _service.GetPriceWithDelta(100d, 0, PriceCalculationMethod.ByBid);
            decimal calculated2 = _service.GetPriceWithDelta(200d, 0, PriceCalculationMethod.ByAsk);

            Assert.AreEqual(calculated1, 100);
            Assert.AreEqual(calculated2, 200);
        }
    }
}
