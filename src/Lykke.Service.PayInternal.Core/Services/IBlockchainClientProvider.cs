namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IBlockchainClientProvider
    {
        IBlockchainApiClient Get(BlockchainType blockchain);
    }
}
