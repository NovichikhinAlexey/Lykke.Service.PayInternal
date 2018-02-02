using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    [Serializable]
    public class NegativeAmountException : Exception
    {
        public NegativeAmountException()
        {
        }

        public NegativeAmountException(decimal amount) : base("Negative amount")
        {
            Amount = amount.ToString(CultureInfo.InvariantCulture);
        }

        public NegativeAmountException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NegativeAmountException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Amount { get; set; }
    }
}
