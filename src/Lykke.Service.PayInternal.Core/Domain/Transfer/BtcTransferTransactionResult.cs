using System;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public class BtcTransferTransactionResult
    {
        public Guid TransactionId { get; set; }

        public string Transaction { get; set; }

        public string Hash { get; set; }

        public Decimal Fee { get; set; }
    }
}
