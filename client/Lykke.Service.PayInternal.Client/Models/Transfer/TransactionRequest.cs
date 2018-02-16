using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models.Transfer
{
    public class TransactionRequest 
    {
        public string TransactionHash { get; set; }
        public List<SourceAmount> SourceAmounts { get; set; }
        public string DestinationAddress { get; set; }
        public int CountConfirm { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}
