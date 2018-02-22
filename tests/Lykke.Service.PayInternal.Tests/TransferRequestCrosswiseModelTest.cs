using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.PayInternal.Tests
{
    [TestClass]
    public class TransferRequestCrosswiseModelTest
    {
        [TestMethod]
        [TestCategory("TransferRequests")]
        public void CheckAmountsValidity_Source_amounts_must_be_greater_than_zero()
        {
            // Arrange
            var testObject = new TransferRequestCrosswiseModel()
            {
                MerchantId = "some-merchant-id",
                FeePayer = TransferFeePayerEnum.Merchant
            };

            testObject.Sources = new List<AddressAmount>();
            testObject.Sources.Add(new AddressAmount() { Address = "some-address", Amount = -5 });
            testObject.Sources.Add(new AddressAmount() { Address = "some-address-2", Amount = 0 });
            testObject.Sources.Add(new AddressAmount() { Address = "some-address-3", Amount = 100 });

            testObject.Destinations = new List<AddressAmount>();
            // the sum of correctly assigned source and dest amounts must be equal to each other in this test
            testObject.Destinations.Add(new AddressAmount() { Address = "some-dest-address", Amount = 100 }); 

            // Act
            var negativeSourceAmountReaction = testObject.CheckAmountsValidity();
            testObject.Sources.RemoveAt(0);
            var zeroSourceAmountReaction = testObject.CheckAmountsValidity();
            testObject.Sources.RemoveAt(0);
            var normalSourceAmountsReaction = testObject.CheckAmountsValidity();

            // Assert
            Assert.IsNotNull(negativeSourceAmountReaction);
            Assert.IsNotNull(zeroSourceAmountReaction);
            Assert.IsNull(normalSourceAmountsReaction);
        }

        [TestMethod]
        [TestCategory("TransferRequests")]
        public void CheckAmountsValidity_Dest_amounts_must_be_greater_than_zero()
        {
            // Arrange
            var testObject = new TransferRequestCrosswiseModel()
            {
                MerchantId = "some-merchant-id",
                FeePayer = TransferFeePayerEnum.Merchant
            };

            testObject.Sources = new List<AddressAmount>();
            testObject.Sources.Add(new AddressAmount() { Address = "some-address", Amount = 50 });
            testObject.Sources.Add(new AddressAmount() { Address = "some-address-2", Amount = 10 });
            testObject.Sources.Add(new AddressAmount() { Address = "some-address-3", Amount = 101 });

            testObject.Destinations = new List<AddressAmount>();
            // the sums of correctly assigned source and dest amounts must be equal to each other in this test
            testObject.Destinations.Add(new AddressAmount() { Address = "some-dest-address", Amount = -100 });
            testObject.Destinations.Add(new AddressAmount() { Address = "some-dest-address-2", Amount = 0 });
            testObject.Destinations.Add(new AddressAmount() { Address = "some-dest-address-3", Amount = 161 });

            // Act
            var negativeDestAmountReaction = testObject.CheckAmountsValidity();
            testObject.Destinations.RemoveAt(0);
            var zeroDestAmountReaction = testObject.CheckAmountsValidity();
            testObject.Destinations.RemoveAt(0);
            var normalDestAmountsReaction = testObject.CheckAmountsValidity();

            // Assert
            Assert.IsNotNull(negativeDestAmountReaction);
            Assert.IsNotNull(zeroDestAmountReaction);
            Assert.IsNull(normalDestAmountsReaction);
        }

        [TestMethod]
        [TestCategory("TransferRequests")]
        public void CheckAmountsValidity_If_source_and_dest_sums_are_not_equal()
        {
            // Arrange
            var testObject = new TransferRequestCrosswiseModel()
            {
                MerchantId = "some-merchant-id",
                FeePayer = TransferFeePayerEnum.Merchant
            };

            testObject.Sources = new List<AddressAmount>();
            testObject.Sources.Add(new AddressAmount() { Address = "some-address", Amount = 20 });
            testObject.Sources.Add(new AddressAmount() { Address = "some-address-2", Amount = 48 });
            testObject.Sources.Add(new AddressAmount() { Address = "some-address-3", Amount = 14 });

            testObject.Destinations = new List<AddressAmount>();
            // the sums of correctly assigned source and dest amounts must NOT be equal to each other in this test
            testObject.Destinations.Add(new AddressAmount() { Address = "some-dest-address", Amount = 10 });
            testObject.Destinations.Add(new AddressAmount() { Address = "some-dest-address-2", Amount = 3 });
            testObject.Destinations.Add(new AddressAmount() { Address = "some-dest-address-3", Amount = 250 });

            // Act
            var notEqualSumsReaction = testObject.CheckAmountsValidity();

            // Assert
            Assert.IsNotNull(notEqualSumsReaction);
        }

        [TestMethod]
        [TestCategory("TransferRequests")]
        public void CheckAmountsValidity_If_source_and_dest_sums_are_equal()
        {
            // Arrange
            var testObject = new TransferRequestCrosswiseModel()
            {
                MerchantId = "some-merchant-id",
                FeePayer = TransferFeePayerEnum.Merchant
            };

            testObject.Sources = new List<AddressAmount>();
            testObject.Sources.Add(new AddressAmount() { Address = "some-address", Amount = 20 });
            testObject.Sources.Add(new AddressAmount() { Address = "some-address-2", Amount = 48 });
            testObject.Sources.Add(new AddressAmount() { Address = "some-address-3", Amount = 14 });

            testObject.Destinations = new List<AddressAmount>();
            // the sums of correctly assigned source and dest amounts must NOT be equal to each other in this test
            testObject.Destinations.Add(new AddressAmount() { Address = "some-dest-address", Amount = 48 });
            testObject.Destinations.Add(new AddressAmount() { Address = "some-dest-address-2", Amount = 14 });
            testObject.Destinations.Add(new AddressAmount() { Address = "some-dest-address-3", Amount = 20 });

            // Act
            var equalSumsReaction = testObject.CheckAmountsValidity();

            // Assert
            Assert.IsNull(equalSumsReaction);
        }

        [TestMethod]
        [TestCategory("TransferRequests")]
        public void ToTransferRequest_Check_the_resulting_transasctions_list()
        {
            // Arrange
            var testObject = new TransferRequestCrosswiseModel()
            {
                MerchantId = "some-merchant-id",
                FeePayer = TransferFeePayerEnum.Merchant
            };

            testObject.Sources = new List<AddressAmount>();
            testObject.Sources.Add(new AddressAmount() { Address = "some-address", Amount = 100 });
            testObject.Sources.Add(new AddressAmount() { Address = "some-address-2", Amount = 10 });
            testObject.Sources.Add(new AddressAmount() { Address = "some-address-3", Amount = 45 });

            testObject.Destinations = new List<AddressAmount>();
            // the sums of correctly assigned source and dest amounts must NOT be equal to each other in this test
            testObject.Destinations.Add(new AddressAmount() { Address = "some-dest-address", Amount = 40 });
            testObject.Destinations.Add(new AddressAmount() { Address = "some-dest-address-2", Amount = 80 });
            testObject.Destinations.Add(new AddressAmount() { Address = "some-dest-address-3", Amount = 35 });

            // Act
            var transferRequest = testObject.ToTransferRequest();

            // Assert
            Assert.AreEqual(transferRequest.TransactionRequests.Count, testObject.Destinations.Count);
            // transaction #1:
            Assert.AreEqual(transferRequest.TransactionRequests[0].SourceAmounts.Count, 1); // Please, revise numbers after revicing source/dest count/amounts
            Assert.AreEqual(
                transferRequest.TransactionRequests[0].SourceAmounts.Sum(item => item.Amount),
                transferRequest.TransactionRequests[0].Amount);
            // transaction #2:
            Assert.AreEqual(transferRequest.TransactionRequests[1].SourceAmounts.Count, 3); 
            Assert.AreEqual(
                transferRequest.TransactionRequests[1].SourceAmounts.Sum(item => item.Amount),
                transferRequest.TransactionRequests[1].Amount);
            // transaction #3:
            Assert.AreEqual(transferRequest.TransactionRequests[2].SourceAmounts.Count, 1); 
            Assert.AreEqual(
                transferRequest.TransactionRequests[2].SourceAmounts.Sum(item => item.Amount),
                transferRequest.TransactionRequests[2].Amount);
            // common sums of source and dest amounts
            Assert.AreEqual(
                transferRequest.TransactionRequests.Sum(item => item.SourceAmounts.Sum(sa => sa.Amount)),
                transferRequest.TransactionRequests.Sum(item => item.Amount));
            // common sums of initial source amounts and transaction source amounts
            Assert.AreEqual(
                testObject.Sources.Sum(item => item.Amount),
                transferRequest.TransactionRequests.Sum(item => item.SourceAmounts.Sum(sa => sa.Amount)));
            // common sums of initial dest amounts and transaction dest amounts
            Assert.AreEqual(
                testObject.Destinations.Sum(item => item.Amount),
                transferRequest.TransactionRequests.Sum(item => item.Amount));
        }
    }
}
