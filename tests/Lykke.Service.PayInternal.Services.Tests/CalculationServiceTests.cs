using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.MarketProfile.Client;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
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
                    Percent = 1,
                    Pips = 20
                },
                _logMock.Object);
        }

        [TestMethod]
        public async Task CalculateBtcAmountFullfillmentAsync_PaidBelow()
        {
            const decimal plan = 10;

            const decimal fact = 9;

            _assetsLocalCacheMock.Setup(o => o.GetAssetByIdAsync(LykkeConstants.BitcoinAssetId))
                .ReturnsAsync(new Asset {Accuracy = BtcAccuracy});

            var fullFillmentStatus = await _service.CalculateBtcAmountFullfillmentAsync(plan, fact);

            Assert.AreEqual(AmountFullFillmentStatus.Below, fullFillmentStatus);
        }

        [TestMethod]
        public async Task CalculateBtcAmountFullfillmentAsync_PaidAbove()
        {
            const decimal plan = 10;

            const decimal fact = 11;

            _assetsLocalCacheMock.Setup(o => o.GetAssetByIdAsync(LykkeConstants.BitcoinAssetId))
                .ReturnsAsync(new Asset {Accuracy = BtcAccuracy});

            var fullFillmentStatus = await _service.CalculateBtcAmountFullfillmentAsync(plan, fact);

            Assert.AreEqual(AmountFullFillmentStatus.Above, fullFillmentStatus);
        }

        [TestMethod]
        public async Task CalculateBtcAmountFullfillmentAsync_PaidExact()
        {
            const decimal plan = 10;

            const decimal fact = 10;

            _assetsLocalCacheMock.Setup(o => o.GetAssetByIdAsync(LykkeConstants.BitcoinAssetId))
                .ReturnsAsync(new Asset {Accuracy = BtcAccuracy});

            var fullFillmentStatus = await _service.CalculateBtcAmountFullfillmentAsync(plan, fact);

            Assert.AreEqual(AmountFullFillmentStatus.Exact, fullFillmentStatus);
        }


        [TestMethod]
        public async Task CalculateBtcAmountFullfillmentAsync_Accuracy_TooBig_Exact()
        {
            const decimal plan = (decimal) 10.000022229873549873459872;

            const decimal fact = (decimal) 10.000022226554828009123654;

            _assetsLocalCacheMock.Setup(o => o.GetAssetByIdAsync(LykkeConstants.BitcoinAssetId))
                .ReturnsAsync(new Asset {Accuracy = BtcAccuracy});

            var fullFillmentStatus = await _service.CalculateBtcAmountFullfillmentAsync(plan, fact);

            Assert.AreEqual(AmountFullFillmentStatus.Exact, fullFillmentStatus);
        }

        [TestMethod]
        public async Task CalculateBtcAmountFullfillmentAsync_Accuracy_Less_Below()
        {
            const decimal plan = (decimal) 10.00002222;

            const decimal fact = (decimal) 10.00002221;

            _assetsLocalCacheMock.Setup(o => o.GetAssetByIdAsync(LykkeConstants.BitcoinAssetId))
                .ReturnsAsync(new Asset {Accuracy = BtcAccuracy});

            var fullFillmentStatus = await _service.CalculateBtcAmountFullfillmentAsync(plan, fact);

            Assert.AreEqual(AmountFullFillmentStatus.Below, fullFillmentStatus);
        }

        [TestMethod]
        public async Task CalculateBtcAmountFullfillmentAsync_Accuracy_Less_Above()
        {
            const decimal plan = (decimal) 10.00002222;

            const decimal fact = (decimal) 10.00002225;

            _assetsLocalCacheMock.Setup(o => o.GetAssetByIdAsync(LykkeConstants.BitcoinAssetId))
                .ReturnsAsync(new Asset {Accuracy = BtcAccuracy});

            var fullFillmentStatus = await _service.CalculateBtcAmountFullfillmentAsync(plan, fact);

            Assert.AreEqual(AmountFullFillmentStatus.Above, fullFillmentStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(NegativeAmountException))]
        public async Task CalculateBtcAmountFullfillmentAsync_Plan_Negative()
        {
            const decimal plan = (decimal) -10.00002222;

            const decimal fact = (decimal) 10.00002222;

            await _service.CalculateBtcAmountFullfillmentAsync(plan, fact);
        }

        [TestMethod]
        [ExpectedException(typeof(NegativeAmountException))]
        public async Task CalculateBtcAmountFullfillmentAsync_Fact_Negative()
        {
            const decimal plan = (decimal) 10.00002222;

            const decimal fact = (decimal) -10.00002222;

            await _service.CalculateBtcAmountFullfillmentAsync(plan, fact);
        }
    }
}
