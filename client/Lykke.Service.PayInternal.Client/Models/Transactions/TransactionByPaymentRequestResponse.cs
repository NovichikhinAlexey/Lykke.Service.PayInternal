namespace Lykke.Service.PayInternal.Client.Models.Transactions
{
    public class TransactionByPaymentRequestResponse
    {
        public string Id { get; set; }

        public string TransactionId { get; set; }

        public string TransferId { get; set; }

        public string PaymentRequestId { get; set; }
        public string WalletAddress { get; set; }
        public string[] SourceWalletAddresses { get; set; }
    }
}
