namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IBcnExplorerResolver
    {
        string GetExplorerUrl(BlockchainType blockchain, string transactionHash);
    }
}
