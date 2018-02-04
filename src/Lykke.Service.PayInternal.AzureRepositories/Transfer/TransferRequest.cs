using System;
using System.Collections.Generic;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.AzureRepositories.Transfer
{
    public class TransferRequest : ITransferRequest
    {
        public string TransferId { get; set; }
        public List<ITransactionRequest> TransactionRequests { get; set; }
        public TransferStatus TransferStatus { get; set; }
        public TransferStatusError TransferStatusError { get; set; }
        public DateTime CreateDate { get; set; }
        public string MerchantId { get; set; }
    }
}
