using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Models
{
    public class UpdateTransactionRequest : IUpdateTransactionRequest
    {
        public string TransactionId { get; set; }
        public string WalletAddress { get; set; }
        public double Amount { get; set; }
        public int Confirmations { get; set; }
        public string BlockId { get; set; }
    }
}
