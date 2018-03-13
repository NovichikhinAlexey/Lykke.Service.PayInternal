using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class NoTransactionsToRefundException : Exception
    {
        public NoTransactionsToRefundException()
        {
        }

        public NoTransactionsToRefundException(string paymentReuqestId) : base("There are no transactions to refund")
        {
            PaymentRequestId = paymentReuqestId;
        }

        public NoTransactionsToRefundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoTransactionsToRefundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string PaymentRequestId { get; set; }
    }
}
