namespace Lykke.Service.PayInternal.Core.Domain.History
{
    public class WalletHistoryCashoutCommand : IWalletHistoryCashoutCommand
    {
        public BlockchainType Blockchain { get; set; }
        public string WalletAddress { get; set; }
        public string AssetId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionHash { get; set; }
        public string DesiredAsset { get; set; }
        public string EmployeeEmail { get; set; }
    }
}
