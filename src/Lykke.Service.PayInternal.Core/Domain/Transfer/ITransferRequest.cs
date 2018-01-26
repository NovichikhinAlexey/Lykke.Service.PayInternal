using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    /// <summary>
    /// Transfer requestr
    /// </summary>
    public interface ITransferRequest
    {
        /// <summary>
        /// Merchant Id
        /// </summary>
        string MerchantId { get; set; }
        /// <summary>
        /// Destination Address
        /// </summary>
        string DestinationAddress { get; set; }
        /// <summary>
        /// Amount. If the amount is 0, then we transfer all money from specify wallets. If list of wallets is empty - transfer all money to destination address.
        /// </summary>
        decimal Amount { get; set; }
        /// <summary>
        /// Currency (Default is BTC)
        /// </summary>
        string Currency { get; set; }
    }
}
