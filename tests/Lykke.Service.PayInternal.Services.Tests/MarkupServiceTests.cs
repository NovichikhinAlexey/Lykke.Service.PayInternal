using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayVolatility.Client;
using Lykke.Service.PayVolatility.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Lykke.Service.PayInternal.Services.Tests
{
    [TestClass]
    public class MarkupServiceTests
    {
        private Mock<IMarkupRepository> _markupRepositoryMock;
        private Mock<IPayVolatilityClient> _payVolatilityClientMock;
        private IMarkupService _markupService;

        [TestInitialize]
        public void Initialize()
        {
            _markupRepositoryMock = SetUpFakeRepository();
            _payVolatilityClientMock = SetUpPayVolatilityClient();
            _markupService = new MarkupService(_markupRepositoryMock.Object, _payVolatilityClientMock.Object);
        }

        public Mock<IPayVolatilityClient> SetUpPayVolatilityClient()
        {
            var mock = new Mock<IPayVolatilityClient>();

            mock.Setup(o => o.GetDailyVolatilityAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null);

            mock.Setup(o => o.GetDailyVolatilitiesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new List<VolatilityModel>());

            return mock;
        }

        public Mock<IMarkupRepository> SetUpFakeRepository()
        {
            var markups = new List<IMarkup>();

            var mock = new Mock<IMarkupRepository>();

            mock.Setup(o => o.SetAsync(It.IsAny<IMarkup>()))
                .ReturnsAsync((IMarkup m) => m)
                .Callback((IMarkup m) => markups.Add(m));

            mock.Setup(
                    o => o.GetByIdentityAsync(It.IsAny<MarkupIdentityType>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((MarkupIdentityType identityType, string identity, string assetPairId) =>
                {
                    return markups.SingleOrDefault(x =>
                        x.AssetPairId == assetPairId && x.IdentityType == identityType &&
                        (identityType == MarkupIdentityType.None || x.Identity == identity));
                });

            mock.Setup(o => o.GetByIdentityAsync(It.IsAny<MarkupIdentityType>(), It.IsAny<string>()))
                .ReturnsAsync((MarkupIdentityType identityType, string identity) =>
                {
                    return markups.Where(x => x.IdentityType == identityType && x.Identity == identity).ToList();
                });

            return mock;
        }

        [TestMethod]
        public async Task SetDefaultAsync_AddsDefaultMarkup()
        {
            const string btcUsd = "BTCUSD";

            IMarkup nullMarkup = await _markupService.GetDefaultAsync(btcUsd);

            await _markupService.SetDefaultAsync(btcUsd, null, PriceMethod.Direct, new MarkupValue());

            IMarkup defaultMarkup = await _markupService.GetDefaultAsync(btcUsd);

            Assert.IsNull(nullMarkup);
            Assert.IsNotNull(defaultMarkup);
            Assert.AreEqual(defaultMarkup.AssetPairId, btcUsd);
            Assert.AreEqual(defaultMarkup.IdentityType, MarkupIdentityType.None);
        }

        [TestMethod]
        public async Task SetForMerchantAsync_AddsMerchantMarkup()
        {
            const string btcUsd = "BTCUSD";

            const string merchant = "SomeMerchant";

            IMarkup nullMarkup = await _markupService.GetForMerchantAsync(merchant, btcUsd);
                
            await _markupService.SetForMerchantAsync(btcUsd, merchant, null, PriceMethod.Direct, new MarkupValue());

            IMarkup merchantMarkup = await _markupService.GetForMerchantAsync(merchant, btcUsd);

            Assert.IsNull(nullMarkup);
            Assert.IsNotNull(merchantMarkup);
            Assert.AreEqual(merchantMarkup.AssetPairId, btcUsd);
            Assert.AreEqual(merchantMarkup.IdentityType, MarkupIdentityType.Merchant);
        }

        [TestMethod]
        public async Task SetDefaultAsync_MultiplePairs()
        {
            const string btcUsd = "BTCUSD";

            const string btcLkk = "BTCLKK";

            await _markupService.SetDefaultAsync(btcUsd, null, PriceMethod.Direct, new MarkupValue());

            await _markupService.SetDefaultAsync(btcLkk, null, PriceMethod.Direct, new MarkupValue());

            IMarkup btcUsdMarkup = await _markupService.GetDefaultAsync(btcUsd);
            IMarkup btcLkkMarkup = await _markupService.GetDefaultAsync(btcLkk);
            IReadOnlyList<IMarkup> defaultMarkups = await _markupService.GetDefaultsAsync();

            Assert.IsNotNull(btcUsdMarkup);
            Assert.IsNotNull(btcLkkMarkup);
            Assert.AreEqual(btcUsdMarkup.AssetPairId, btcUsd);
            Assert.AreEqual(btcUsdMarkup.IdentityType, MarkupIdentityType.None);
            Assert.AreEqual(btcLkkMarkup.AssetPairId, btcLkk);
            Assert.AreEqual(btcLkkMarkup.IdentityType, MarkupIdentityType.None);
            Assert.IsTrue(defaultMarkups.Count == 2);
            Assert.IsTrue(defaultMarkups.Contains(btcUsdMarkup));
            Assert.IsTrue(defaultMarkups.Contains(btcLkkMarkup));
        }

        [TestMethod]
        public async Task SetForMerchantAsync_MultiplePairs()
        {
            const string btcUsd = "BTCUSD";

            const string btcLkk = "BTCLKK";

            const string merchant = "SomeMerchant";

            await _markupService.SetForMerchantAsync(btcUsd, merchant, null, PriceMethod.Direct, new MarkupValue());

            await _markupService.SetForMerchantAsync(btcLkk, merchant, null, PriceMethod.Direct, new MarkupValue());

            IMarkup btcUsdMarkup = await _markupService.GetForMerchantAsync(merchant, btcUsd);
            IMarkup btcLkkMarkup = await _markupService.GetForMerchantAsync(merchant, btcLkk);
            IReadOnlyList<IMarkup> merchantMarkups = await _markupService.GetForMerchantAsync(merchant);

            Assert.IsNotNull(btcUsdMarkup);
            Assert.IsNotNull(btcLkkMarkup);
            Assert.AreEqual(btcUsdMarkup.AssetPairId, btcUsd);
            Assert.AreEqual(btcUsdMarkup.IdentityType, MarkupIdentityType.Merchant);
            Assert.AreEqual(btcUsdMarkup.Identity, merchant);
            Assert.AreEqual(btcLkkMarkup.AssetPairId, btcLkk);
            Assert.AreEqual(btcLkkMarkup.IdentityType, MarkupIdentityType.Merchant);
            Assert.AreEqual(btcLkkMarkup.Identity, merchant);
            Assert.IsTrue(merchantMarkups.Count == 2);
            Assert.IsTrue(merchantMarkups.Contains(btcUsdMarkup));
            Assert.IsTrue(merchantMarkups.Contains(btcLkkMarkup));
        }

        [TestMethod]
        public async Task ResolveAsync_DefaultOnly()
        {
            const string btcUsd = "BTCUSD";

            const string merchant = "SomeMerchant";

            IMarkup defaultMarkup =
                await _markupService.SetDefaultAsync(btcUsd, null, PriceMethod.Direct, new MarkupValue());

            IMarkup resolvedMarkup = await _markupService.ResolveAsync(merchant, btcUsd);

            Assert.IsNotNull(defaultMarkup);
            Assert.IsNotNull(resolvedMarkup);
            Assert.AreEqual(defaultMarkup, resolvedMarkup);
        }

        [TestMethod]
        public async Task ResolveAsync_MerchantOnly()
        {
            const string btcUsd = "BTCUSD";

            const string merchant = "SomeMerchant";

            IMarkup defaultMarkup =
                await _markupService.SetForMerchantAsync(btcUsd, merchant, null, PriceMethod.Direct, new MarkupValue());

            IMarkup resolvedMarkup = await _markupService.ResolveAsync(merchant, btcUsd);

            Assert.IsNotNull(defaultMarkup);
            Assert.IsNotNull(resolvedMarkup);
            Assert.AreEqual(defaultMarkup, resolvedMarkup);
        }

        [TestMethod]
        public async Task ResolveAsync_MultipleMarkups()
        {
            const string btcUsd = "BTCUSD";

            const string merchant = "SomeMerchant";

            IMarkup defaultMarkup =
                await _markupService.SetDefaultAsync(btcUsd, null, PriceMethod.Direct, new MarkupValue());

            IMarkup merchantMarkup =
                await _markupService.SetForMerchantAsync(btcUsd, merchant, null, PriceMethod.Reverse, new MarkupValue());

            IMarkup resolvedMarkup = await _markupService.ResolveAsync(merchant, btcUsd);

            Assert.IsNotNull(defaultMarkup);
            Assert.IsNotNull(merchantMarkup);
            Assert.IsNotNull(resolvedMarkup);
            Assert.AreNotEqual(defaultMarkup, resolvedMarkup);
            Assert.AreEqual(merchantMarkup, resolvedMarkup);
        }

        [TestMethod]
        [ExpectedException(typeof(MarkupNotFoundException))]
        public async Task ResolveAsync_NoMarkup_ThrowsException()
        {
            const string btcUsd = "BTCUSD";

            const string merchant = "SomeMerchant";

            await _markupService.ResolveAsync(merchant, btcUsd);
        }

        [TestMethod]
        [ExpectedException(typeof(MarkupNotFoundException))]
        public async Task ResolveAsync_AnotherMarkup_ThrowsException()
        {
            const string btcUsd = "BTCUSD";

            const string merchant1 = "SomeMerchant1";

            const string merchant2 = "SomeMerchant2";

            await _markupService.SetForMerchantAsync(btcUsd, merchant1, null, PriceMethod.Direct, new MarkupValue());

            await _markupService.ResolveAsync(merchant2, btcUsd);
        }
    }
}
