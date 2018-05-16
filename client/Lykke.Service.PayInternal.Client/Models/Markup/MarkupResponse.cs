namespace Lykke.Service.PayInternal.Client.Models.Markup
{
    public class MarkupResponse
    {
        public string AssetPairId { get; set; }

        public decimal DeltaSpread { get; set; }

        public decimal Percent { get; set; }

        public int Pips { get; set; }

        public decimal FixedFee { get; set; }

        public string PriceAssetPairId { get; set; }

        public PriceMethod PriceMethod { get; set; }
    }
}
