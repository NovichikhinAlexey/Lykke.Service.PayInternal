using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lykke.Service.PayInternal.Tests
{
    [TestClass]
    public class TransferRequestMultiBijectiveModelTest
    {
        [TestMethod]
        [TestCategory("TransferRequests")]
        public void CheckAmountsValidity_Amounts_must_be_greater_than_zero()
        {
            // Arrange
            var testObject = new TransferRequestMultiBijectiveModel()
            {
                MerchantId = "some-merchant-id",
                FeePayer = TransferFeePayerEnum.Merchant
            };

            testObject.BiAddresses = new List<BiAddressAmount>();
            testObject.BiAddresses.Add(new BiAddressAmount() { SourceAddress = "some-source-address", DestinationAddress = "some-dest-address", Amount = -10 });
            testObject.BiAddresses.Add(new BiAddressAmount() { SourceAddress = "some-source-address-2", DestinationAddress = "some-dest-address-2", Amount = 0 });
            testObject.BiAddresses.Add(new BiAddressAmount() { SourceAddress = "some-source-address-3", DestinationAddress = "some-dest-address-3", Amount = 100 });
            testObject.BiAddresses.Add(new BiAddressAmount() { SourceAddress = "some-source-address-4", DestinationAddress = "some-dest-address-4", Amount = 120 });

            // Act
            var negativeAmountReaction = testObject.CheckAmountsValidity();
            testObject.BiAddresses.RemoveAt(0);
            var zeroAmountReaction = testObject.CheckAmountsValidity();
            testObject.BiAddresses.RemoveAt(0);
            var normalAmountsReaction = testObject.CheckAmountsValidity();

            // Asert
            Assert.IsNotNull(negativeAmountReaction);
            Assert.IsNotNull(zeroAmountReaction);
            Assert.IsNull(normalAmountsReaction);
        }

        [TestMethod]
        [TestCategory("TransferRequests")]
        public void ToTransferRequest_Transactions_count_must_be_equal_to_biaddresses_count()
        {
            // Arrange
            var testObject = new TransferRequestMultiBijectiveModel()
            {
                MerchantId = "some-merchant-id",
                FeePayer = TransferFeePayerEnum.Merchant
            };

            testObject.BiAddresses = new List<BiAddressAmount>();
            testObject.BiAddresses.Add(new BiAddressAmount() { SourceAddress = "some-source-address", DestinationAddress = "some-dest-address", Amount = 29 });
            testObject.BiAddresses.Add(new BiAddressAmount() { SourceAddress = "some-source-address-2", DestinationAddress = "some-dest-address-2", Amount = 87 });
            testObject.BiAddresses.Add(new BiAddressAmount() { SourceAddress = "some-source-address-3", DestinationAddress = "some-dest-address-3", Amount = 105 });
            testObject.BiAddresses.Add(new BiAddressAmount() { SourceAddress = "some-source-address-4", DestinationAddress = "some-dest-address-4", Amount = 10 });

            // Act
            var transferRequest = testObject.ToTransferRequest();

            // Asert
            Assert.AreEqual(
                testObject.BiAddresses.Count,
                transferRequest.TransactionRequests.Count);
            Assert.AreEqual(
                testObject.BiAddresses.Sum(item => item.Amount),
                transferRequest.TransactionRequests.Sum(item => item.Amount));
        }
    }
}
