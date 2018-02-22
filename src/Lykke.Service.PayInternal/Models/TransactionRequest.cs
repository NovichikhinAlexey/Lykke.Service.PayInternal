using System.Collections.Generic;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Models
{
    public class TransactionRequest : ITransactionRequest
    {
        public string TransactionHash { get; set; }
        public List<IAddressAmount> SourceAmounts { get; set; }
        public string DestinationAddress { get; set; }
        public int CountConfirm { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}
