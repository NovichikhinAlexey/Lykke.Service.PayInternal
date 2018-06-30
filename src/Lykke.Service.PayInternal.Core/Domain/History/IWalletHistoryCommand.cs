namespace Lykke.Service.PayInternal.Core.Domain.History
{
    public interface IWalletHistoryCommand
    {
        BlockchainType Blockchain { get; set; }

        string WalletAddress { get; set; }

        string AssetId { get; set; }

        decimal Amount { get; set; }

        string MerchantId { get; set; }

        string TransactionHash { get; set; }
    }
}
