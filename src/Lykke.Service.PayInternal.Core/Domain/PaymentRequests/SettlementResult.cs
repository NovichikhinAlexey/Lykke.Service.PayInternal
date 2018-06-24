namespace Lykke.Service.PayInternal.Core.Domain.PaymentRequests
{
    public class SettlementResult
    {
        public string WalletAddress { get; set; }
        public BlockchainType Blockchain { get; set; }
        public string AssetId { get; set; }
        public decimal Amount { get; set; }
    }
}
