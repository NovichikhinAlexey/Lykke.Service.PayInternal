using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public class TransferResult
    {
        public string Id { get; set; }

        public BlockchainType Blockchain { get; set; }

        public IEnumerable<TransferTransactionResult> Transactions { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
