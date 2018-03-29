using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Client.Models.Transactions
{
    public class TransactionByPaymentRequestResponse
    {
        string Id { get; }

        string TransactionId { get; set; }

        string TransferId { get; set; }

        string PaymentRequestId { get; set; }
        string WalletAddress { get; set; }
        string[] SourceWalletAddresses { get; set; }
    }
}
