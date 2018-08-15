using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public class BlockchainTransferCommand
    {
        public IList<TransferAmount> Amounts { get; set; }

        public string AssetId { get; set; }

        public BlockchainTransferCommand(string assetId)
        {
            AssetId = assetId;
            Amounts = new List<TransferAmount>();
        }
    }
}
