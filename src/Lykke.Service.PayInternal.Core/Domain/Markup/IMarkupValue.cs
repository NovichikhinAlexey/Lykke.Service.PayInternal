namespace Lykke.Service.PayInternal.Core.Domain.Markup
{
    public interface IMarkupValue
    {
        decimal DeltaSpread { get; set; }

        decimal Percent { get; set; }

        int Pips { get; set; }

        decimal FixedFee { get; set; }
    }
}
