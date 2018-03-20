using System;
using System.Collections.Generic;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class Transfer : ITransfer
    {
        public string Id { get; }

        public string AssetId { get; set; }

        public string Blockchain { get; set; }

        public IEnumerable<TransferAmount> Amounts { get; set; }

        public IEnumerable<TransferTransaction> Transactions { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
