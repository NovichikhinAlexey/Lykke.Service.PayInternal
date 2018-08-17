using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Orders;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Order = Lykke.Service.PayInternal.Services.Domain.Order;

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

            mock.Setup(o => o.GetByPaymentRequestAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string paymentRequestId, string orderId) =>
                {
                    return orders.SingleOrDefault(x => x.PaymentRequestId == paymentRequestId && x.Id == orderId);
                });

            mock.Setup(o => o.GetByPaymentRequestAsync(It.IsAny<string>()))
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
    }
}
