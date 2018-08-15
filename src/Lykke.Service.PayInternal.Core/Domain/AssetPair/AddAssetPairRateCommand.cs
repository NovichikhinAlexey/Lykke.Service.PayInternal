namespace Lykke.Service.PayInternal.Core.Domain.AssetPair
{
    public class AddAssetPairRateCommand
    {
        public string BaseAssetId { get; set; }

        public string QuotingAssetId { get; set; }

        public decimal BidPrice { get; set; }

        public decimal AskPrice { get; set; }
    }
}
