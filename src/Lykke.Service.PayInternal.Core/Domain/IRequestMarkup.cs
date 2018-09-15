namespace Lykke.Service.PayInternal.Core.Domain
{
    public interface IRequestMarkup
    {
        decimal FixedFee { get; set; }

        decimal Percent { get; set; }

        int Pips { get; set; }
    }
}
