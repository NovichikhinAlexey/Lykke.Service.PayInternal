namespace Lykke.Service.PayInternal.Client.Models.Transactions
{
    public class CreateLykkeTransactionRequest
    {
        public string OperationId { get; set; }
        public string[] SourceWalletAddresses { get; set; }
        public decimal Amount { get; set; }
        public string AssetId { get; set; }
        public int Confirmations { get; set; }
        public TransactionIdentityType IdentityType { get; set; }
        public string Identity { get; set; }
    }
}
