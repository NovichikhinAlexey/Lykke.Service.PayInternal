namespace Lykke.Service.PayInternal.Core.Domain
{
    public interface IWalletAddressHolder : IBlockchainTypeHolder
    {
        string WalletAddress { get; set; }
    }
}
