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


            // Act

            // Assert
        }

        [TestMethod]
        [TestCategory("TransferRequests")]
        public void CheckAmountsValidity_Dest_amounts_must_be_greater_than_zero()
        {
            // Arrange

            // Act

            // Assert
        }

        [TestMethod]
        [TestCategory("TransferRequests")]
        public void CheckAmountsValidity_If_source_and_dest_sums_are_not_equal()
        {
            // Arrange

            // Act

            // Assert
        }

        [TestMethod]
        [TestCategory("TransferRequests")]
        public void CheckAmountsValidity_If_source_and_dest_sums_are_equal()
        {
            // Arrange

            // Act

            // Assert
        }

        [TestMethod]
        [TestCategory("TransferRequests")]
        public void ToTransferRequest_Check_the_resulting_transasctions_list()
        {
            // Arrange

            // Act

            // Assert
        }
    }
}
