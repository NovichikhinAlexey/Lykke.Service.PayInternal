using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Lykke.Service.PayInternal.Services.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Lykke.Service.PayInternal.Services.Tests
{
    [TestClass]
    public class OrderServiceTests
    {
        private Mock<IOrderRepository> _orderRepositoryMock;
        private Mock<IMerchantRepository> _merchantRepositoryMock;
        private Mock<ICalculationService> _calculationServiceMock;
        private Mock<IMarkupService> _markupServiceMock;
        private Mock<ILykkeAssetsResolver> _lykkeAssetsResolverMock;

        private IOrderService _orderService;

        [TestInitialize]
        public void Initialize()
        {
            _orderRepositoryMock = SetUpOrderRepository();
            _merchantRepositoryMock = new Mock<IMerchantRepository>();
            _calculationServiceMock = new Mock<ICalculationService>();
            _markupServiceMock = new Mock<IMarkupService>();
            _lykkeAssetsResolverMock = new Mock<ILykkeAssetsResolver>();

            _orderService = new OrderService(
                _orderRepositoryMock.Object,
                _merchantRepositoryMock.Object,
                _calculationServiceMock.Object,
                EmptyLogFactory.Instance,
                new OrderExpirationPeriodsSettings(),
                _markupServiceMock.Object,
                _lykkeAssetsResolverMock.Object);
        }

        public Mock<IOrderRepository> SetUpOrderRepository()
        {
            var orders = new List<IOrder>();

            var mock = new Mock<IOrderRepository>();

            mock.Setup(o => o.InsertAsync(It.IsAny<IOrder>()))
                .ReturnsAsync((IOrder o) => o)
                .Callback((IOrder o) => orders.Add(o));

            mock.Setup(o => o.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string paymentRequestId, string orderId) =>
                {
                    return orders.SingleOrDefault(x => x.PaymentRequestId == paymentRequestId && x.Id == orderId);
                });

            mock.Setup(o => o.GetAsync(It.IsAny<string>()))
                .ReturnsAsync((string paymentRequestId) =>
                {
                    return orders.Where(x => x.PaymentRequestId == paymentRequestId).ToList();
                });

            return mock;
        }

        [TestMethod]
        public async Task GetActualAsync_SingleOrder()
        {
            const string paymentRequestId = "SomePaymentRequestId";

            IOrder dueOrder = await _orderRepositoryMock.Object.InsertAsync(new Order
            {
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                DueDate = DateTime.UtcNow.AddDays(-2),
                ExtendedDueDate = DateTime.UtcNow.AddDays(-1),
                PaymentRequestId = paymentRequestId
            });

            IOrder actualOrder = await _orderRepositoryMock.Object.InsertAsync(new Order
            {
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                DueDate = DateTime.UtcNow.AddDays(-1),
                ExtendedDueDate = DateTime.UtcNow.AddDays(1),
                PaymentRequestId = paymentRequestId
            });

            IOrder resolvedActualOrder = await _orderService.GetActualAsync(paymentRequestId, DateTime.UtcNow);

            Assert.IsNotNull(dueOrder);
            Assert.IsNotNull(actualOrder);
            Assert.AreNotEqual(dueOrder, actualOrder);
            Assert.AreEqual(actualOrder, resolvedActualOrder);
        }

        [TestMethod]
        public async Task GetActualAsync_ForPast()
        {
            const string paymentRequestId = "SomePaymentRequestId";

            IOrder dueOrder = await _orderRepositoryMock.Object.InsertAsync(new Order
            {
                CreatedDate = DateTime.UtcNow.AddDays(-4),
                DueDate = DateTime.UtcNow.AddDays(-3),
                ExtendedDueDate = DateTime.UtcNow.AddDays(-1),
                PaymentRequestId = paymentRequestId
            });

            IOrder actualOrder = await _orderRepositoryMock.Object.InsertAsync(new Order
            {
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                DueDate = DateTime.UtcNow.AddDays(-1),
                ExtendedDueDate = DateTime.UtcNow.AddDays(1),
                PaymentRequestId = paymentRequestId
            });

            IOrder resolvedActualOrder =
                await _orderService.GetActualAsync(paymentRequestId, DateTime.UtcNow.AddDays(-2));

            Assert.IsNotNull(dueOrder);
            Assert.IsNotNull(actualOrder);
            Assert.AreNotEqual(dueOrder, actualOrder);
            Assert.AreEqual(dueOrder, resolvedActualOrder);
        }

        [TestMethod]
        public async Task GetActualAsync_MultipleOrders()
        {
            const string paymentRequestId = "SomePaymentRequestId";

            IOrder dueOrder = await _orderRepositoryMock.Object.InsertAsync(new Order
            {
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                DueDate = DateTime.UtcNow.AddDays(-2),
                ExtendedDueDate = DateTime.UtcNow.AddDays(-1),
                PaymentRequestId = paymentRequestId
            });

            IOrder actualOrder1 = await _orderRepositoryMock.Object.InsertAsync(new Order
            {
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                DueDate = DateTime.UtcNow.AddDays(-1),
                ExtendedDueDate = DateTime.UtcNow.AddDays(2),
                PaymentRequestId = paymentRequestId
            });

            IOrder actualOrder2 = await _orderRepositoryMock.Object.InsertAsync(new Order
            {
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                DueDate = DateTime.UtcNow.AddDays(-1),
                ExtendedDueDate = DateTime.UtcNow.AddDays(4),
                PaymentRequestId = paymentRequestId
            });

            IOrder nullResolvedOrder1 = await _orderService.GetActualAsync("FakePaymentRequestId", DateTime.UtcNow);
            IOrder nullResolvedOrder2 =
                await _orderService.GetActualAsync(paymentRequestId, DateTime.UtcNow.AddDays(5));

            IOrder resolvedActualOrder1 =
                await _orderService.GetActualAsync(paymentRequestId, DateTime.UtcNow.AddDays(1));
            IOrder resolvedActualOrder2 =
                await _orderService.GetActualAsync(paymentRequestId, DateTime.UtcNow.AddDays(3));

            Assert.IsNotNull(dueOrder);
            Assert.IsNull(nullResolvedOrder1);
            Assert.IsNull(nullResolvedOrder2);
            Assert.IsNotNull(resolvedActualOrder1);
            Assert.IsNotNull(resolvedActualOrder2);
            Assert.AreNotEqual(resolvedActualOrder1, resolvedActualOrder2);
            Assert.AreEqual(actualOrder1, resolvedActualOrder1);
            Assert.AreEqual(actualOrder2, resolvedActualOrder2);
        }

        [TestMethod]
        public async Task GetLatestOrCreateAsync_NotForcing_NotExpiredDueDate_ReturnsActual()
        {
            IPaymentRequest paymentRequest = new PaymentRequest
            {
                Id = "SomePaymentRequestId"
            };

            IOrder actualOrder = await _orderRepositoryMock.Object.InsertAsync(new Order
            {
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                DueDate = DateTime.UtcNow.AddDays(1),
                ExtendedDueDate = DateTime.UtcNow.AddDays(2),
                PaymentRequestId = paymentRequest.Id
            });

            IOrder resolvedOrder = await _orderService.GetLatestOrCreateAsync(paymentRequest);

            Assert.IsNotNull(actualOrder);
            Assert.IsNotNull(resolvedOrder);
            Assert.AreEqual(actualOrder, resolvedOrder);
        }

        [TestMethod]
        public async Task GetLatestOrCreateAsync_NotForcing_ExpiredDueDate_ReturnsActual()
        {
            IPaymentRequest paymentRequest = new PaymentRequest
            {
                Id = "SomePaymentRequestId"
            };

            IOrder actualOrder = await _orderRepositoryMock.Object.InsertAsync(new Order
            {
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                DueDate = DateTime.UtcNow.AddDays(-1),
                ExtendedDueDate = DateTime.UtcNow.AddDays(1),
                PaymentRequestId = paymentRequest.Id
            });

            IOrder resolvedOrder = await _orderService.GetLatestOrCreateAsync(paymentRequest);

            Assert.IsNotNull(actualOrder);
            Assert.IsNotNull(resolvedOrder);
            Assert.AreEqual(actualOrder, resolvedOrder);
        }

        [TestMethod]
        public async Task GetLatestOrCreateAsync_ExpiredDueDate_ExpiredExtendedDueDate_CreatesNewOrder()
        {
            const string btcUsd = "BTCUSD";

            const decimal paymentAmount = 100;

            const decimal rate = 10;

            var partiallyMockedOrderService = new Mock<OrderService>(
                _orderRepositoryMock.Object,
                _merchantRepositoryMock.Object,
                _calculationServiceMock.Object,
                EmptyLogFactory.Instance,
                new OrderExpirationPeriodsSettings
                {
                    Primary = TimeSpan.FromDays(1),
                    Extended = TimeSpan.FromDays(2)
                },
                _markupServiceMock.Object,
                _lykkeAssetsResolverMock.Object)
            {
                CallBase = true
            };

            partiallyMockedOrderService.Setup(
                    o => o.GetPaymentInfoAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<decimal>(),
                        It.IsAny<string>(),
                        It.IsAny<IRequestMarkup>()))
                .ReturnsAsync(() => (btcUsd, paymentAmount, rate));

            IPaymentRequest paymentRequest = new PaymentRequest
            {
                Id = "SomePaymentRequestId"
            };

            IOrder dueOrder = await _orderRepositoryMock.Object.InsertAsync(new Order
            {
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                DueDate = DateTime.UtcNow.AddDays(-2),
                ExtendedDueDate = DateTime.UtcNow.AddDays(-1),
                PaymentRequestId = paymentRequest.Id
            });

            IOrder actualOrder = await partiallyMockedOrderService.Object.GetLatestOrCreateAsync(paymentRequest);

            Assert.IsNotNull(dueOrder);
            Assert.IsNotNull(actualOrder);
            Assert.AreNotEqual(dueOrder, actualOrder);
            Assert.IsTrue(DateTime.UtcNow < actualOrder.DueDate);
            Assert.IsTrue(DateTime.UtcNow < actualOrder.ExtendedDueDate);
            Assert.IsTrue(actualOrder.DueDate < actualOrder.ExtendedDueDate);
            Assert.IsTrue(actualOrder.CreatedDate < actualOrder.DueDate);
            Assert.AreEqual(actualOrder.AssetPairId, btcUsd);
            Assert.AreEqual(actualOrder.PaymentAmount, paymentAmount);
            Assert.AreEqual(actualOrder.ExchangeRate, rate);
        }
    }
}
