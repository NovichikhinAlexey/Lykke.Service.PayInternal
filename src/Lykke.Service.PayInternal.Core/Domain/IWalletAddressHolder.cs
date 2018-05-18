namespace Lykke.Service.PayInternal.Core.Domain
{
    public interface IWalletAddressHolder
    {
        string WalletAddress { get; set; }

        BlockchainType Blockchain { get; set; }
    }
}
