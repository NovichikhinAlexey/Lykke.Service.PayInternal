namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public interface IWalletEntity
    {
        string MerchantId { get; set; }
        string Address { get; set; }
        string Data { get; set; }
    }
}
