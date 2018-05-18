namespace Lykke.Service.PayInternal.Client.Models.Transactions
{
    public class TransactionExpiredRequest
    {
        public BlockchainType Blockchain { get; set; }
        public TransactionIdentityType IdentityType { get; set; }
        public string Identity { get; set; }
    }
}
