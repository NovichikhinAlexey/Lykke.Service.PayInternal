namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IAutoSettleSettingsResolver
    {
        bool AllowToMakePartialAutoSettle(string assetId);
        bool AllowToSettleToMerchantWallet(string assetId);
        string GetAutoSettleWallet(BlockchainType blockchain);
    }
}
