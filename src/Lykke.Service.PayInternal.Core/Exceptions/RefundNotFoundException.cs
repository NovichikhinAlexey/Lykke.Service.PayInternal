using System;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class RefundNotFoundException : Exception
    {
        public string MerchantId { get; }
        public string RefundId { get; }

        public RefundNotFoundException(string merchantId, string refundId)
            : base("Refund request not found.")
        {
            MerchantId = merchantId;
            RefundId = refundId;
        }
    }
}
