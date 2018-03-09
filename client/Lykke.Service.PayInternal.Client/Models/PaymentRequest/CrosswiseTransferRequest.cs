using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    public class CrosswiseTransferRequest
    {
        public string PaymentRequestId { get; set; }
        public string MerchantId { get; set; }
        public string AssetId { get; set; }
        public int FeeRate { get; set; }
        public decimal FixedFee { get; set; }
        public List<AddressAmount> Sources { get; set; }
        public List<AddressAmount> Destinations { get; set; }
    }

    public class AddressAmount
    {
        public string Address { get; set; }
        public decimal Amount { get; set; }
    }
}
