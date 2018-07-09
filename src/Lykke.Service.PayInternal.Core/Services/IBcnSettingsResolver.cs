namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IBcnSettingsResolver
    {
        string GetExplorerUrl(BlockchainType blockchain, string transactionHash);

        string GetExchangeHotWallet(BlockchainType blockchain);

        string GetCashoutHotWallet(BlockchainType blockchain);
    }
}
