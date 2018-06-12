namespace Lykke.Service.PayInternal.Core.Domain
{
    public class MerchantWalletBalanceLine
    {
        public string Id { get; set; }
        public string AssetId { get; set; }
        public decimal Balance { get; set; }
    }
}
