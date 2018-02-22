namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    /// <summary>
    /// Abstract definition for entity describing what amount of asset should be transfered from/to the mentioned address.
    /// </summary>
    public interface IAddressAmount
    {
        /// <summary>
        /// Address (source/destination)
        /// </summary>
        string Address { get; set; }
        /// <summary>
        /// Amount of asset to transfer
        /// </summary>
        decimal Amount { get; set; }
    }
}
