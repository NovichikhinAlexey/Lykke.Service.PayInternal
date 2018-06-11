namespace Lykke.Service.PayInternal.Core.Domain.MerchantWallet
{
    public interface IMerchantWalletBalance
    {
        IMerchantWallet Wallet { get; set; }

        decimal Balance { get; set; }
    }
}
