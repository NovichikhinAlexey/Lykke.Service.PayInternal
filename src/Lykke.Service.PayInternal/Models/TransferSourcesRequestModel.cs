using System.Collections.Generic;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Models
{
    public class TransferSourcesRequestModel : TransferRequestModel
    {
        public IEnumerable<ISourceAmount> SourceAddresses { get; set; }
    }
}
