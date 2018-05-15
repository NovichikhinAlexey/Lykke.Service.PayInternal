using Lykke.Service.PayInternal.Core.Domain.Markup;

namespace Lykke.Service.PayInternal.Models.Markups
{
    public class MarkupResponse : IMarkupValue
    {
        public string AssetPairId { get; set; }

        public decimal DeltaSpread { get; set; }

        public decimal Percent { get; set; }

        public int Pips { get; set; }

        public decimal FixedFee { get; set; }
    }
}
