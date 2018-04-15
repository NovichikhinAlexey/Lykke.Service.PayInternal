namespace Lykke.Service.PayInternal.Client.Models.Transactions
{
    public class TransactionExpiredRequest
    {
        public string TransactionId { get; set; }

        public BlockchainType Blockchain { get; set; }
    }
}
