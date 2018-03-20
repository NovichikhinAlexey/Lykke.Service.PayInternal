using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public class TransferResult
    {
        public string Id { get; set; }

        public string Blockchain { get; set; }

        public IEnumerable<TransferTransactionResult> Transactions { get; set; }
    }
}
