namespace Lykke.Service.PayInternal.Core.Domain.Cashout
{
    public class CashoutResult
    {
        public string SourceWalletAddress { get; set; }

        public string AssetId { get; set; }

        public decimal Amount { get; set; }

        public string DestWalletAddress { get; set; }
    }
}
