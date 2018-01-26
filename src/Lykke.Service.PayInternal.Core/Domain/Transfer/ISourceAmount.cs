namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    /// <summary>
    /// Source - amount pair
    /// </summary>
    public interface ISourceAmount
    {
        /// <summary>
        /// Source Address
        /// </summary>
        string SourceAddress { get; set; }
        /// <summary>
        /// Amount
        /// </summary>
        decimal Amount { get; set; }
    }
}
