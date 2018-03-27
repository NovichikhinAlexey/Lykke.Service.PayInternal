using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public interface ITransfer
    {
        string Id { get; set; }

        string AssetId { get; set; }

        string Blockchain { get; set; }

        IEnumerable<TransferAmount> Amounts { get; set; }

        IEnumerable<TransferTransaction> Transactions { get; set; }

        DateTime CreatedOn { get; set; }
    }
}
