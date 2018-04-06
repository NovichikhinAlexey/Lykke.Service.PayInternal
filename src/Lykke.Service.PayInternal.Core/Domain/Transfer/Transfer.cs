using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public class Transfer : ITransfer
    {
        public string Id { get; set; }

        public string AssetId { get; set; }

        public BlockchainType Blockchain { get; set; }

        public IEnumerable<TransferAmount> Amounts { get; set; }

        public IEnumerable<TransferTransaction> Transactions { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
