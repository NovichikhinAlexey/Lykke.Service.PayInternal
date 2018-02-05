using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    [Serializable]
    public class NegativeValueException : Exception
    {
        public NegativeValueException()
        {
        }

        public NegativeValueException(decimal amount) : base("Negative amount")
        {
            Amount = amount.ToString(CultureInfo.InvariantCulture);
        }

        public NegativeValueException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NegativeValueException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Amount { get; set; }
    }
}
