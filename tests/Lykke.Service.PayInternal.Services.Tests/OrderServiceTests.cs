using System.Collections.Generic;
using Lykke.Logs;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
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

            return mock;
        }
    }
}
