using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class InvalidRowKeyValueException : Exception
    {
        public InvalidRowKeyValueException()
        {
        }

        public InvalidRowKeyValueException(string variable, string value) : base("Invalid row key or partition key value")
        {
            Variable = variable;
            Value = value;
        }

        public InvalidRowKeyValueException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidRowKeyValueException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Variable { get; set; }
        public string Value { get; set; }
    }
}
