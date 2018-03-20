namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IFeeProvider
    {
        int FeeRate { get; }

        decimal FixedFee { get; }
    }
}
