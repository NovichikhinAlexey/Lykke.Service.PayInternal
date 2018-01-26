namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public interface ISourceAmount
    {
        string SourceAddress { get; set; }
        double Amount { get; set; }
    }
}
