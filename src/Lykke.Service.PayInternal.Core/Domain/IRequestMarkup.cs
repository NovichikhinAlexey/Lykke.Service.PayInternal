namespace Lykke.Service.PayInternal.Core.Domain
{
    public interface IRequestMarkup
    {
        double FixedFee { get; set; }

        double Percent { get; set; }

        int Pips { get; set; }
    }
}
