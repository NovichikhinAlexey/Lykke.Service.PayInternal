namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface IUpdateTransaction
    {
        string TransactionId { get; set; }
        string WalletAddress { get; set; }
        decimal Amount { get; set; }
        int Confirmations { get; set; }
        string BlockId { get; set; }
    }
}
