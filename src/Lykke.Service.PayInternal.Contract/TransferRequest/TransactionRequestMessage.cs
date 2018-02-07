using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Contract.TransferRequest
{
    public class TransactionRequestMessage
    {
        public TransactionRequestMessage()
        {
            SourceAmounts = new List<SourceAmountMessage>();
        }
        public string TransactionHash { get; set; }
        public List<SourceAmountMessage> SourceAmounts { get; set; }
        public string DestinationAddress { get; set; }
        public int CountConfirm { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}
