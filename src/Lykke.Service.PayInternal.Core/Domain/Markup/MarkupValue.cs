namespace Lykke.Service.PayInternal.Core.Domain.Markup
{
    public class MarkupValue : IMarkupValue
    {
        public decimal DeltaSpread { get; set; }
        public decimal Percent { get; set; }
        public int Pips { get; set; }
        public decimal FixedFee { get; set; }
    }
}
