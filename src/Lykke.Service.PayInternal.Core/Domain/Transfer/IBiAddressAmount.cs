using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    /// <summary>
    /// Abstract definition for entity describing what amount of asset should be transfered from AND to the mentioned addresses.
    /// </summary>
    public interface IBiAddressAmount
    {
        /// <summary>
        /// Source address
        /// </summary>
        string SourceAddress { get; set; }
        /// <summary>
        /// Destination address
        /// </summary>
        string DestinationAddress { get; set; }
        /// <summary>
        /// Amount of asset to transfer
        /// </summary>
        decimal Amount { get; set; }
    }
}
