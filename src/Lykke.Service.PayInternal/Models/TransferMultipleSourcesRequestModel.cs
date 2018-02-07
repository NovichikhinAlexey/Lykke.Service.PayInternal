using System.Collections.Generic;
using System.Linq;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Models
{
    public class TransferMultipleSourcesRequestModel : TransferRequestModel
    {
        public List<SourceAmount> SourceAddresses { get; set; }

        public override ITransferRequest ToTransferRequest()
        {
            var result = base.ToTransferRequest();
            result.TransactionRequests.First().SourceAmounts = new List<ISourceAmount>(
                from source in SourceAddresses
                select new SourceAmount
                {
                    SourceAddress = source.SourceAddress,
                    Amount = source.Amount
                });
            return result;
        }
    }
}
