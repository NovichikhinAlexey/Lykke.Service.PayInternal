using System.Collections.Generic;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Models
{
    public class TransferSourcesRequestModel : TransferRequestModel, ISourcesTransferRequest
    {
        public IEnumerable<ISourceAmount> SourceAddresses { get; set; }
    }
}
