using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class NotEnoughMoneyException : Exception
    {
        public NotEnoughMoneyException()
        {
        }

        public NotEnoughMoneyException(string message) : base(message)
        {
        }

        public NotEnoughMoneyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotEnoughMoneyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public NotEnoughMoneyException(decimal available, decimal required) : base("Not enough money")
        {
            Available = available;

            Required = required;
        }

        public decimal Available { get; set; }

        public decimal Required { get; set; }
    }
}
