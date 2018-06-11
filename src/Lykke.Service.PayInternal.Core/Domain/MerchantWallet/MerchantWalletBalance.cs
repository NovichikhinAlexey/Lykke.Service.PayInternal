namespace Lykke.Service.PayInternal.Core.Domain.MerchantWallet
{
    public class MerchantWalletBalance : IMerchantWalletBalance
    {
        public IMerchantWallet Wallet { get; set; }
        public decimal Balance { get; set; }
    }
}
