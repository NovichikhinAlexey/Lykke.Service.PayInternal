namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public interface IWallet
    {
        string MerchantId { get; set; }
        string Address { get; set; }
        string Data { get; set; }
    }
}
