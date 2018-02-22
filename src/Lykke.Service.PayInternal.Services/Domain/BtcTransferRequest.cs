using System;
using System.Collections.Generic;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class BtcTransferRequest : ITransferRequest
    {
        public static ITransferRequest CreateErrorTransferRequest(string merchantId, TransferStatusError statusError, List<ITransactionRequest> transactions)
        {
            return new BtcTransferRequest
            {
                TransferId = Guid.NewGuid().ToString(),
                TransactionRequests = transactions,
                TransferStatus = TransferStatus.Error,
                TransferStatusError = statusError,
                MerchantId = merchantId
            };
        }

        public static ITransferRequest CreateTransferRequest(string merchantId, List<ITransactionRequest> transactions)
        {
            return new BtcTransferRequest
            {
                TransferId = Guid.NewGuid().ToString(),
                TransactionRequests = transactions,
                TransferStatus = TransferStatus.InProgress,
                TransferStatusError = TransferStatusError.NotError,
                MerchantId = merchantId,
                CreateDate = DateTime.Now
            };
        }

        /// <summary>
        /// --Currently not implemented-- Creates a deep copy of this btc transfer request.
        /// </summary>
        /// <returns></returns>
        public ITransferRequest DeepCopy()
        {
            throw new NotImplementedException();
        }

        public string TransferId { get; set; }
        public List<ITransactionRequest> TransactionRequests { get; set; }
        public TransferStatus TransferStatus { get; set; }
        public TransferStatusError TransferStatusError { get; set; }
        public DateTime CreateDate { get; set; }
        public string MerchantId { get; set; }
    }

    
}
