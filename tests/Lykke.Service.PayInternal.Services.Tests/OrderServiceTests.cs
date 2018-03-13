using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Lykke.Service.PayInternal.Services.Tests
{
    [TestClass]
    public class OrderServiceTests
    {
        private const int TransactionConfirmationsCount = 3;
        private readonly TimeSpan _orderExpiration = TimeSpan.FromMinutes(10);

        private Mock<IOrderRepository> _orderRepositoryMock;
        private Mock<IAssetsLocalCache> _assetsLocalCacheMock;
        private Mock<IMerchantRepository> _merchantRepositoryMock;
        private Mock<ICalculationService> _calculationServiceMock;
        private Mock<ILog> _logMock;

        private IOrderService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _assetsLocalCacheMock = new Mock<IAssetsLocalCache>();
            _merchantRepositoryMock = new Mock<IMerchantRepository>();
            _calculationServiceMock = new Mock<ICalculationService>();
            _logMock = new Mock<ILog>();

            _service = new OrderService(
                _orderRepositoryMock.Object,
                _assetsLocalCacheMock.Object,
                _merchantRepositoryMock.Object,
                _calculationServiceMock.Object,
                _logMock.Object,
                _orderExpiration,
                TransactionConfirmationsCount);
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedAssetException))]
        public async Task GetPaymentStatus_Status_Any_Not_Btc()
        {
            var paymentRequest = new PaymentRequest
            {
                Id = "pr1",
                Status = PaymentRequestStatus.New
            };

            var blockchainTransaction = new BlockchainTransaction
            {
                Amount = 10,
                AssetId = "USD",
                FirstSeen = DateTime.UtcNow
            };
            
            // act
            await _service.GetPaymentRequestStatus(new List<IBlockchainTransaction> {blockchainTransaction}, paymentRequest.Id);
        }

        [TestMethod]
        public async Task GetPaymentStatus_Status_New_No_Transactions()
        {
            var paymentRequest = new PaymentRequest
            {
                Id = "pr1",
                Status = PaymentRequestStatus.New
            };
            // act
            var paymentStatus =
                await _service.GetPaymentRequestStatus(Enumerable.Empty<IBlockchainTransaction>().ToList(), paymentRequest.Id);

            // assert
            Assert.AreEqual(PaymentRequestStatus.New, paymentStatus.Status);
        }

        [TestMethod]
        public async Task GetPaymentStatus_Status_New_Detected_First_Transaction_Status_Sets_To_InProcess()
        {
            var paymentRequest = new PaymentRequest
            {
                Id = "pr1",
                Status = PaymentRequestStatus.New
            };

            var blockchainTransaction = new BlockchainTransaction
            {
                Amount = 10,
                AssetId = "BTC",
                FirstSeen = DateTime.UtcNow
            };

            _orderRepositoryMock.Setup(o => o.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<IOrder>
                {
                    new Order
                    {
                        AssetPairId = "BTCCHF",
                        PaymentAmount = 10,
                        DueDate = DateTime.UtcNow.AddDays(1)
                    }
                });
            
            // act
            var paymentStatus =
                await _service.GetPaymentRequestStatus(new List<IBlockchainTransaction> {blockchainTransaction}, paymentRequest.Id);

            // assert
            Assert.AreEqual(PaymentRequestStatus.InProcess, paymentStatus.Status);
        }

        [TestMethod]
        public async Task GetPaymentStatus_Status_InProcess_Move_To_Confirmed()
        {
            var paymentRequest = new PaymentRequest
            {
                Id = "pr1",
                Status = PaymentRequestStatus.New
            };

            var blockchainTransaction = new BlockchainTransaction
            {
                Amount = 10,
                AssetId = "BTC",
                FirstSeen = DateTime.UtcNow,
                Confirmations = TransactionConfirmationsCount
            };

            _orderRepositoryMock.Setup(o => o.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<IOrder>
                {
                    new Order
                    {
                        AssetPairId = "BTCCHF",
                        PaymentAmount = 10,
                        DueDate = DateTime.UtcNow.AddDays(1)
                    }
                });

            _calculationServiceMock.Setup(o =>
                    o.CalculateBtcAmountFullfillmentAsync(It.IsAny<decimal>(), It.IsAny<decimal>()))
                .ReturnsAsync(AmountFullFillmentStatus.Exact);
            
            // act
            var paymentStatus =
                await _service.GetPaymentRequestStatus(new List<IBlockchainTransaction> {blockchainTransaction}, paymentRequest.Id);

            // assert
            Assert.AreEqual(PaymentRequestStatus.Confirmed, paymentStatus.Status);
        }

        [TestMethod]
        public async Task GetPaymentStatus_Status_InProcess_Move_To_Error_Amount_Bellow()
        {
            var paymentRequest = new PaymentRequest
            {
                Id = "pr1",
                Status = PaymentRequestStatus.New
            };

            var blockchainTransaction = new BlockchainTransaction
            {
                Amount = 9,
                AssetId = "BTC",
                FirstSeen = DateTime.UtcNow,
                Confirmations = TransactionConfirmationsCount
            };

            _orderRepositoryMock.Setup(o => o.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<IOrder>
                {
                    new Order
                    {
                        AssetPairId = "BTCCHF",
                        PaymentAmount = 10,
                        DueDate = DateTime.UtcNow.AddDays(1)
                    }
                });

            _calculationServiceMock.Setup(o =>
                    o.CalculateBtcAmountFullfillmentAsync(It.IsAny<decimal>(), It.IsAny<decimal>()))
                .ReturnsAsync(AmountFullFillmentStatus.Below);
            
            // act
            var paymentStatus =
                await _service.GetPaymentRequestStatus(new List<IBlockchainTransaction> {blockchainTransaction}, paymentRequest.Id);

            // assert
            Assert.AreEqual(PaymentRequestStatus.Error, paymentStatus.Status);
            Assert.AreEqual(paymentStatus.Details, "AMOUNT BELOW");
        }

        [TestMethod]
        public async Task GetPaymentStatus_Status_InProcess_Move_To_Error_Amount_Above()
        {
            var paymentRequest = new PaymentRequest
            {
                Id = "pr1",
                Status = PaymentRequestStatus.New
            };

            var blockchainTransaction = new BlockchainTransaction
            {
                Amount = 11,
                AssetId = "BTC",
                FirstSeen = DateTime.UtcNow,
                Confirmations = TransactionConfirmationsCount
            };

            _orderRepositoryMock.Setup(o => o.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<IOrder>
                {
                    new Order
                    {
                        AssetPairId = "BTCCHF",
                        PaymentAmount = 10,
                        DueDate = DateTime.UtcNow.AddDays(1)
                    }
                });

            _calculationServiceMock.Setup(o =>
                    o.CalculateBtcAmountFullfillmentAsync(It.IsAny<decimal>(), It.IsAny<decimal>()))
                .ReturnsAsync(AmountFullFillmentStatus.Above);
            
            // act
            var paymentStatus =
                await _service.GetPaymentRequestStatus(new List<IBlockchainTransaction> {blockchainTransaction}, paymentRequest.Id);

            // assert
            Assert.AreEqual(PaymentRequestStatus.Error, paymentStatus.Status);
            Assert.AreEqual(paymentStatus.Details, "AMOUNT ABOVE");
        }

        [TestMethod]
        public async Task GetPaymentStatus_Status_InProcess_Move_To_Error_Expired()
        {
            var paymentRequest = new PaymentRequest
            {
                Id = "pr1",
                Status = PaymentRequestStatus.New
            };

            var blockchainTransaction = new BlockchainTransaction
            {
                Amount = 10,
                AssetId = "BTC",
                FirstSeen = DateTime.UtcNow,
                Confirmations = TransactionConfirmationsCount
            };

            _orderRepositoryMock.Setup(o => o.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<IOrder>
                {
                    new Order
                    {
                        AssetPairId = "BTCCHF",
                        PaymentAmount = 10,
                        DueDate = DateTime.UtcNow.AddDays(-1)
                    }
                });

            _calculationServiceMock.Setup(o =>
                    o.CalculateBtcAmountFullfillmentAsync(It.IsAny<decimal>(), It.IsAny<decimal>()))
                .ReturnsAsync(AmountFullFillmentStatus.Exact);
            
            // act
            var paymentStatus =
                await _service.GetPaymentRequestStatus(new List<IBlockchainTransaction> {blockchainTransaction}, paymentRequest.Id);

            // assert
            Assert.AreEqual(PaymentRequestStatus.Error, paymentStatus.Status);
            Assert.AreEqual(paymentStatus.Details, "EXPIRED");
        }
    }
}
