using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    public class MultiBijectiveTransferRequest
    {
        public string PaymentRequestId { get; set; }
        public string MerchantId { get; set; }
        public string AssetId { get; set; }
        public int FeeRate { get; set; }
        public decimal FixedFee { get; set; }
        public List<BiAddressAmount> BiAddresses { get; set; }
    }

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
