namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public enum BlockchainTransactionStatus
    {
        Unknown,
        New,
        InProgress,
        Confirmed
    }

    public interface IBlockchainTransaction
    {
        string Id { get; }
        string OrderId { get; set; }
        double Amount { get; set; }
        string BlockId { get; set; }
        int Confirmations { get; set; }
        BlockchainTransactionStatus Status { get; set; }
        string WalletAddress { get; set; }
    }
}
