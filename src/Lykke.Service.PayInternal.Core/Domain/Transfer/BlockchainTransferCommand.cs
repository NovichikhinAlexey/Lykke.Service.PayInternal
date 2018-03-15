using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public class BlockchainTransferCommand
    {
        public IEnumerable<TransferAmount> Amounts { get; set; }

        public string AssetId { get; set; }
    }
}
