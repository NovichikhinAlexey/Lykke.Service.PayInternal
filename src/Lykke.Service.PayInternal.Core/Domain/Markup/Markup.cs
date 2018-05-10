using System;

namespace Lykke.Service.PayInternal.Core.Domain.Markup
{
    public class Markup : IMarkup
    {
        public decimal DeltaSpread { get; set; }
        public decimal Percent { get; set; }
        public int Pips { get; set; }
        public decimal FixedFee { get; set; }
        public string AssetPairId { get; set; }
        public MarkupIdentityType IdentityType { get; set; }
        public string Identity { get; set; }
        public DateTime CreatedOn { get; set; }
        public string PriceAssetPairId { get; set; }
        public PriceMethod PriceMethod { get; set; }
    }
}
