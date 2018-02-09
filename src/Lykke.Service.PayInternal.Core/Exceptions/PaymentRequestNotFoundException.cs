using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when requested payment request cannot be found.
    /// </summary>
    [Serializable]
    public class PaymentRequestNotFoundException : Exception
    {
        public PaymentRequestNotFoundException()
        {
        }

        public PaymentRequestNotFoundException(string walletAddress)
            : base("Payment request not found.")
        {
            WalletAddress = walletAddress;
        }
        
        public PaymentRequestNotFoundException(string merchantId, string paymentRequestId)
            : base("Payment request not found.")
        {
            MerchantId = merchantId;
            PaymentRequestId = paymentRequestId;
        }

        public PaymentRequestNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PaymentRequestNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string MerchantId { get; }
        
        public string PaymentRequestId { get; }
        
        public string WalletAddress { get; }
    }
}
