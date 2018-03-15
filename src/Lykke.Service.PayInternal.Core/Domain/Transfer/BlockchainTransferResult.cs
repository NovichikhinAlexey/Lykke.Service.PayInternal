using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public class BlockchainTransferResult
    {
        public string Blockchain { get; set; }

        public IList<BlockchainTransactionResult> Transactions { get; set; }

        public BlockchainTransferResult()
        {
            Transactions = new List<BlockchainTransactionResult>();
        }
    }
}
