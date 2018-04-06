using System;
using System.Runtime.Serialization;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class WalletAlreadyExistsException : Exception
    {
        public WalletAlreadyExistsException()
        {
        }

        public WalletAlreadyExistsException(string walletId) : base("Wallet already exists")
        {
            WalletId = walletId;
        }

        public WalletAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WalletAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string WalletId { get; set; }
    }
}
