using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public interface ITransactionRequest
    {

        string TransactionHash { get; set; }
        List<IAddressAmount> SourceAmounts { get; set; }
        string DestinationAddress { get; set; }
        int CountConfirm { get; set; }
        decimal Amount { get; set; }
        string Currency { get; set; }
    }
}
