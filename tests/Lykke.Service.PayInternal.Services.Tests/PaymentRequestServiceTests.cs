using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Lykke.Service.PayInternal.Services.Tests
{
    [TestClass]
    public class PaymentRequestServiceTests
    {
        private const int TransactionConfirmationsCount = 3;
        
        private Mock<IPaymentRequestRepository> _paymentRequestRepositoryMock;
        private Mock<IMerchantWalletsService> _merchantWalletsServiceMock;
        private Mock<IBlockchainTransactionRepository> _transactionRepositoryMock;
        private Mock<IOrderService> _orderServiceMock;
        private Mock<IPaymentRequestPublisher> _paymentRequestPublisherMock;
        private Mock<IAssetsLocalCache> _assetsLocalCacheMock;
        private Mock<ILog> _logMock;
        private Mock<ICalculationService> _calculationServiceMock;

        private PaymentRequestService _service;
        
        [TestInitialize]
        public void TestInitialized()
        {
            _paymentRequestRepositoryMock = new Mock<IPaymentRequestRepository>();
            _merchantWalletsServiceMock = new Mock<IMerchantWalletsService>();
            _transactionRepositoryMock = new Mock<IBlockchainTransactionRepository>();
            _orderServiceMock = new Mock<IOrderService>();
            _paymentRequestPublisherMock = new Mock<IPaymentRequestPublisher>();
            _assetsLocalCacheMock = new Mock<IAssetsLocalCache>();
            _logMock = new Mock<ILog>();
            _calculationServiceMock = new Mock<ICalculationService>();
            
            _service = new PaymentRequestService(
                _paymentRequestRepositoryMock.Object,
                _merchantWalletsServiceMock.Object,
                _transactionRepositoryMock.Object,
                _orderServiceMock.Object,
                _paymentRequestPublisherMock.Object,
                _assetsLocalCacheMock.Object,
                _logMock.Object,
                TransactionConfirmationsCount,
                _calculationServiceMock.Object);
        }
        
        [TestMethod]
        public async Task ProcessAsync_Status_New_Detected_First_Transaction_Status_Sets_To_InProcess()
        {
            // arrange
            const string walletAddress = "wa";
            
            var paymentRequest = new PaymentRequest
            {
                Id = "pr1",
                Status = PaymentRequestStatus.New
            };
            
            var blockchainTransaction = new BlockchainTransaction();

            PaymentRequestStatus status = PaymentRequestStatus.None;
            
            _paymentRequestRepositoryMock.Setup(o => o.FindAsync(It.IsAny<string>()))
                .ReturnsAsync(paymentRequest);
            _transactionRepositoryMock.Setup(o => o.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<IBlockchainTransaction> {blockchainTransaction});
            _paymentRequestRepositoryMock.Setup(o => o.UpdateAsync(It.IsAny<IPaymentRequest>()))
                .Returns(Task.CompletedTask)
                .Callback((IPaymentRequest o) => status = o.Status);
            
            // act
            await _service.ProcessAsync(walletAddress);

            // assert
            Assert.AreEqual(PaymentRequestStatus.InProcess, status);
        }

        [TestMethod]
        public async Task ProcessAsync_Status_InProcess_Move_To_Confirmed()
        {
            // arrange
            const string walletAddress = "wa";
            const decimal amount = 0.0394825m;
            const string assetPairId = "BTCCHF";
            
            var paymentRequest = new PaymentRequest
            {
                Id = "pr1",
                Status = PaymentRequestStatus.New
            };
            
            var blockchainTransaction = new BlockchainTransaction
            {
                Amount = amount * (decimal)Math.Pow(10, 8),
                FirstSeen = DateTime.Now,
                Confirmations = TransactionConfirmationsCount
            };

            var order = new Order
            {
                PaymentAmount = amount,
                DueDate = DateTime.Now.AddMinutes(1),
                AssetPairId = assetPairId
            };

            var assetPair = new AssetPair
            {
                Id = assetPairId,
                Accuracy = 8
            };

            PaymentRequestStatus status = PaymentRequestStatus.None;
            
            _paymentRequestRepositoryMock.Setup(o => o.FindAsync(It.IsAny<string>()))
                .ReturnsAsync(paymentRequest);
            _transactionRepositoryMock.Setup(o => o.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<IBlockchainTransaction> {blockchainTransaction});
            _orderServiceMock.Setup(o => o.GetAsync(It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(order);
            _assetsLocalCacheMock.Setup(o => o.GetAssetPairByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(assetPair);
            
            _paymentRequestRepositoryMock.Setup(o => o.UpdateAsync(It.IsAny<IPaymentRequest>()))
                .Returns(Task.CompletedTask)
                .Callback((IPaymentRequest o) => status = o.Status);
            
            // act
            await _service.ProcessAsync(walletAddress);

            // assert
            Assert.AreEqual(PaymentRequestStatus.Confirmed, status);
        }
        
        [TestMethod]
        public async Task ProcessAsync_Status_InProcess_Move_To_Error_Amount_Bellow()
        {
            // arrange
            const string walletAddress = "wa";
            const decimal amount = 0.0394825m;
            const string assetPairId = "BTCCHF";
            
            var paymentRequest = new PaymentRequest
            {
                Id = "pr1",
                Status = PaymentRequestStatus.New
            };
            
            var blockchainTransaction = new BlockchainTransaction
            {
                Amount = (amount - .0001m) * (decimal)Math.Pow(10, 8),
                FirstSeen = DateTime.Now,
                Confirmations = TransactionConfirmationsCount
            };

            var order = new Order
            {
                PaymentAmount = amount,
                DueDate = DateTime.Now.AddMinutes(1),
                AssetPairId = assetPairId
            };

            var assetPair = new AssetPair
            {
                Id = assetPairId,
                Accuracy = 8
            };

            PaymentRequestStatus status = PaymentRequestStatus.None;
            string error = null;
            
            _paymentRequestRepositoryMock.Setup(o => o.FindAsync(It.IsAny<string>()))
                .ReturnsAsync(paymentRequest);
            _transactionRepositoryMock.Setup(o => o.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<IBlockchainTransaction> {blockchainTransaction});
            _orderServiceMock.Setup(o => o.GetAsync(It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(order);
            _assetsLocalCacheMock.Setup(o => o.GetAssetPairByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(assetPair);
            
            _paymentRequestRepositoryMock.Setup(o => o.UpdateAsync(It.IsAny<IPaymentRequest>()))
                .Returns(Task.CompletedTask)
                .Callback((IPaymentRequest o) =>
                {
                    status = o.Status;
                    error = o.Error;
                });
            
            // act
            await _service.ProcessAsync(walletAddress);

            // assert
            Assert.IsTrue(PaymentRequestStatus.Error == status && error == "AMOUNT BELOW");
        }
        
        [TestMethod]
        public async Task ProcessAsync_Status_InProcess_Move_To_Error_Amount_Above()
        {
            // arrange
            const string walletAddress = "wa";
            const decimal amount = 0.0394825m;
            const string assetPairId = "BTCCHF";
            
            var paymentRequest = new PaymentRequest
            {
                Id = "pr1",
                Status = PaymentRequestStatus.New
            };
            
            var blockchainTransaction = new BlockchainTransaction
            {
                Amount = (amount + .0001m) * (decimal)Math.Pow(10, 8),
                FirstSeen = DateTime.Now,
                Confirmations = TransactionConfirmationsCount
            };

            var order = new Order
            {
                PaymentAmount = amount,
                DueDate = DateTime.Now.AddMinutes(1),
                AssetPairId = assetPairId
            };

            var assetPair = new AssetPair
            {
                Id = assetPairId,
                Accuracy = 8
            };

            PaymentRequestStatus status = PaymentRequestStatus.None;
            string error = null;
            
            _paymentRequestRepositoryMock.Setup(o => o.FindAsync(It.IsAny<string>()))
                .ReturnsAsync(paymentRequest);
            _transactionRepositoryMock.Setup(o => o.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<IBlockchainTransaction> {blockchainTransaction});
            _orderServiceMock.Setup(o => o.GetAsync(It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(order);
            _assetsLocalCacheMock.Setup(o => o.GetAssetPairByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(assetPair);
            
            _paymentRequestRepositoryMock.Setup(o => o.UpdateAsync(It.IsAny<IPaymentRequest>()))
                .Returns(Task.CompletedTask)
                .Callback((IPaymentRequest o) =>
                {
                    status = o.Status;
                    error = o.Error;
                });
            
            // act
            await _service.ProcessAsync(walletAddress);

            // assert
            Assert.IsTrue(PaymentRequestStatus.Error == status && error == "AMOUNT ABOVE");
        }
        
        [TestMethod]
        public async Task ProcessAsync_Status_InProcess_Move_To_Error_Expired()
        {
            // arrange
            const string walletAddress = "wa";
            const decimal amount = 0.0394825m;
            const string assetPairId = "BTCCHF";
            
            var paymentRequest = new PaymentRequest
            {
                Id = "pr1",
                Status = PaymentRequestStatus.New
            };
            
            var blockchainTransaction = new BlockchainTransaction
            {
                Amount = amount,
                FirstSeen = DateTime.Now.AddMinutes(1),
                Confirmations = TransactionConfirmationsCount
            };

            var assetPair = new AssetPair
            {
                Id = assetPairId,
                Accuracy = 8
            };

            PaymentRequestStatus status = PaymentRequestStatus.None;
            string error = null;
            
            _paymentRequestRepositoryMock.Setup(o => o.FindAsync(It.IsAny<string>()))
                .ReturnsAsync(paymentRequest);
            _transactionRepositoryMock.Setup(o => o.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<IBlockchainTransaction> {blockchainTransaction});
            _orderServiceMock.Setup(o => o.GetAsync(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult<IOrder>(null));
            _assetsLocalCacheMock.Setup(o => o.GetAssetPairByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(assetPair);
            
            _paymentRequestRepositoryMock.Setup(o => o.UpdateAsync(It.IsAny<IPaymentRequest>()))
                .Returns(Task.CompletedTask)
                .Callback((IPaymentRequest o) =>
                {
                    status = o.Status;
                    error = o.Error;
                });
            
            // act
            await _service.ProcessAsync(walletAddress);

            // assert
            Assert.IsTrue(PaymentRequestStatus.Error == status && error == "EXPIRED");
        }
    }
}
