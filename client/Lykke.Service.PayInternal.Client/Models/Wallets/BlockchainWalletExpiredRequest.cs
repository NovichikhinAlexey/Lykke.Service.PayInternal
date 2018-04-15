namespace Lykke.Service.PayInternal.Client.Models.Wallets
{
    public class BlockchainWalletExpiredRequest
    {
        public string WalletAddress { get; set; }
        public BlockchainType Blockchain { get; set; }
    }
}
