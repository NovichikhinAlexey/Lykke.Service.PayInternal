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

            // Act

            // Asert
        }

        [TestMethod]
        [TestCategory("TransferRequests")]
        public void ToTransferRequest_Transactions_count_must_be_equal_to_biaddresses_count()
        {
            // Arrange

            // Act

            // Asert
        }
    }
}
