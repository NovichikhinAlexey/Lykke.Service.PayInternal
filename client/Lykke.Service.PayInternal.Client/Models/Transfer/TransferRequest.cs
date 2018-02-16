using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models.Transfer
{
    public class TransferRequest 
    {
        public string TransferId { get; set; }
        public List<TransactionRequest> TransactionRequests { get; set; }
        public TransferStatus TransferStatus { get; set; }
        public TransferStatusError TransferStatusError { get; set; }
        public DateTime CreateDate { get; set; }
        public string MerchantId { get; set; }
    }
}
