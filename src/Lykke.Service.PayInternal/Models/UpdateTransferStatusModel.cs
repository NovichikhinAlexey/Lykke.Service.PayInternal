using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Models
{
    public class UpdateTransferStatusModel : ITransfer
    {
        public string TransferId { get; set; }
        public string TransactionHash { get; set; }
        public TransferStatus TransferStatus { get; set; }
        public TransferStatusError TransferStatusError { get; set; }
    }
}
