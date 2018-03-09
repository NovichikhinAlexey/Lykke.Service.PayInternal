using JetBrains.Annotations;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    [UsedImplicitly]
    public class BiAddressAmount
    {
        public string SourceAddress { get; set; }
        /// <summary>
        /// Destination address
        /// </summary>
        public string DestinationAddress { get; set; }
        /// <summary>
        /// Amount of asset to transfer
        /// </summary>
        public decimal Amount { get; set; }
    }
}
