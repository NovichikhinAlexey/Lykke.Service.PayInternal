using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.MarketProfile.Client;
using Lykke.Service.PayInternal.Core.Domain.AssetPair;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Lykke.Service.PayInternal.Services.Tests
{
    [TestClass]
    public class AssetRatesServiceTests
    {
        private Mock<IAssetPairRateRepository> _assetPairRateRepositoryMock;
        private Mock<ILykkeMarketProfile> _lykkeMarketProfileMock;
        private Mock<IAssetsLocalCache> _assetsLocalCacheMock;
        private Mock<IAssetPairSettingsService> _assetPairSettingsServiceMock;

        private IAssetRatesService _assetRatesService;

        [TestInitialize]
        public void TestInitialize()
        {
            _assetPairRateRepositoryMock = new Mock<IAssetPairRateRepository>();
            _lykkeMarketProfileMock = new Mock<ILykkeMarketProfile>();
            _assetsLocalCacheMock = new Mock<IAssetsLocalCache>();
            _assetPairSettingsServiceMock = new Mock<IAssetPairSettingsService>();

            _assetRatesService = new AssetRatesService(
                _assetPairRateRepositoryMock.Object,
                _lykkeMarketProfileMock.Object,
                _assetsLocalCacheMock.Object,
                _assetPairSettingsServiceMock.Object);
        }

        [TestMethod]
        public async Task AddAsync_InvertedRateAdded()
        {
            const decimal bidPrice = 1;

            const decimal askPrice = 2;

            const string baseAssetId = "BTC";

            const string quotingAssetId = "LKK";

            const int assetPairAccuracy = 1;

            var rates = new List<IAssetPairRate>();

            _assetPairSettingsServiceMock.Setup(o => o.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(
                new AssetPairSetting
                {
                    BaseAssetId = baseAssetId,
                    QuotingAssetId = quotingAssetId,
                    Accuracy = assetPairAccuracy
                });

            _assetPairRateRepositoryMock.Setup(o => o.AddAsync(It.IsAny<AssetPairRate>()))
                .ReturnsAsync((IAssetPairRate rate) => rate)
                .Callback((IAssetPairRate rate) => rates.Add(rate));

            var newRateCommand = new AddAssetPairRateCommand
            {
                BidPrice = bidPrice,
                AskPrice = askPrice,
                BaseAssetId = baseAssetId,
                QuotingAssetId = quotingAssetId,
            };

            await _assetRatesService.AddAsync(newRateCommand);

            Assert.IsNotNull(rates.Single(x =>
                x.BaseAssetId == newRateCommand.BaseAssetId && x.QuotingAssetId == newRateCommand.QuotingAssetId));
            Assert.IsNotNull(rates.Single(x =>
                x.BaseAssetId == newRateCommand.QuotingAssetId && x.QuotingAssetId == newRateCommand.BaseAssetId));
            Assert.IsTrue(rates.Single(x => x.BaseAssetId == baseAssetId).BidPrice.Equals(newRateCommand.BidPrice));
            Assert.IsTrue(rates.Single(x => x.BaseAssetId == baseAssetId).AskPrice.Equals(newRateCommand.AskPrice));
            Assert.IsTrue(rates.Single(x => x.BaseAssetId == quotingAssetId).BidPrice
                .Equals(newRateCommand.BidPrice > 0 ? 1 / newRateCommand.BidPrice : 0));
            Assert.IsTrue(rates.Single(x => x.BaseAssetId == quotingAssetId).AskPrice
                .Equals(newRateCommand.AskPrice > 0 ? 1 / newRateCommand.AskPrice : 0));
        }

        [TestMethod]
        [ExpectedException(typeof(AssetPairRateStorageNotSupportedException))]
        public async Task AddAsync_NoSettings_ThrowsException()
        {
            const decimal bidPrice = 1;

            const decimal askPrice = 2;

            const string baseAssetId = "BTC";

            const string quotingAssetId = "LKK";

            _assetPairSettingsServiceMock.Setup(o => o.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(() => null);

            var newRateCommand = new AddAssetPairRateCommand
            {
                BidPrice = bidPrice,
                AskPrice = askPrice,
                BaseAssetId = baseAssetId,
                QuotingAssetId = quotingAssetId,
            };

            await _assetRatesService.AddAsync(newRateCommand);
        }

        [TestMethod]
        public async Task GetCurrentRateAsync_LocalRates_ReturnsLatest()
        {
            const decimal currentBid = 20;

            const decimal currentAsk = 40;

            const string btcAsset = "BTC";

            const string lkkAsset = "LKK";

            var rates = new List<IAssetPairRate>
            {
                new AssetPairRate
                {
                    CreatedOn = DateTime.UtcNow.AddDays(-10),
                    BaseAssetId = btcAsset,
                    QuotingAssetId = lkkAsset,
                    BidPrice = 10,
                    AskPrice = 20
                },
                new AssetPairRate
                {
                    CreatedOn = DateTime.UtcNow.AddDays(-5),
                    BaseAssetId = btcAsset,
                    QuotingAssetId = lkkAsset,
                    BidPrice = currentBid,
                    AskPrice = currentAsk
                }
            };

            _assetPairSettingsServiceMock.Setup(o => o.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(() => new AssetPairSetting());

            _assetPairRateRepositoryMock.Setup(o => o.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string baseAssetId, string quotingAssetId) => rates
                    .Where(x => x.BaseAssetId == baseAssetId && x.QuotingAssetId == quotingAssetId).ToList());

            IAssetPairRate currentRate = await _assetRatesService.GetCurrentRateAsync(btcAsset, lkkAsset);
            IAssetPairRate failedCurrentRate = await _assetRatesService.GetCurrentRateAsync(lkkAsset, btcAsset);

            Assert.IsNotNull(currentRate);
            Assert.IsNull(failedCurrentRate);
            Assert.AreEqual(currentRate.BaseAssetId, btcAsset);
            Assert.AreEqual(currentRate.QuotingAssetId, lkkAsset);
            Assert.AreEqual(currentRate.BidPrice, currentBid);
            Assert.AreEqual(currentRate.AskPrice, currentAsk);
        }

        [TestMethod]
        [ExpectedException(typeof(AssetPairUnknownException))]
        public async Task GetCurrentRateAsync_UnknownAssetPair_ThrowsException()
        {
            _assetPairSettingsServiceMock.Setup(o => o.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(() => null);

            _assetsLocalCacheMock.Setup(o => o.GetAssetPairAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(() => null);

            await _assetRatesService.GetCurrentRateAsync("fakeAsset1", "fakeAsset2");
        }
    }
}
