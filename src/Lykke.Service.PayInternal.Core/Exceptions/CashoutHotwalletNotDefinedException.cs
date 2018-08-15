using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class CashoutHotwalletNotDefinedException : Exception
    {
        public CashoutHotwalletNotDefinedException()
        {
        }

        public CashoutHotwalletNotDefinedException(BlockchainType blockchain) : base("Cashout hotwallet not defined")
        {
            Blockchain = blockchain;
        }

        public CashoutHotwalletNotDefinedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CashoutHotwalletNotDefinedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BlockchainType Blockchain { get; set; }
    }
}
