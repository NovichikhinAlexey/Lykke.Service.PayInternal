using System.Collections.Generic;
using System.Linq;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Models
{
    public class TransferSingleSourceRequestModel : TransferRequestModel
    {
        public string SourceAddress { get; set; }

        public override ITransferRequest ToTransferRequest()
        {
            var result = base.ToTransferRequest();
            result.TransactionRequests.First().SourceAmounts = new List<ISourceAmount>()
            {
                new SourceAmount
                {
                    SourceAddress = SourceAddress,
                    Amount = 0
                }
            };

            return result;
        }

        
    }
}
