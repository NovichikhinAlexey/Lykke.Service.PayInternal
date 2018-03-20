using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class MultiTransactionRefundNotSupportedException : Exception
    {
        public MultiTransactionRefundNotSupportedException()
        {
        }

        public MultiTransactionRefundNotSupportedException(int transactionCount) : base("Multitransaction refund not supported")
        {
        }

        public MultiTransactionRefundNotSupportedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MultiTransactionRefundNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public int TransactionCount { get; set; }
    }
}
