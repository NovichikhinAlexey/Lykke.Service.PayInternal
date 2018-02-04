using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Models
{
    public class UpdateTransferStatusModel
    {
        public string TransferId { get; set; }
        public string TransactionHash { get; set; }
        public TransferStatus TransferStatus { get; set; }
        public TransferStatusError TransferStatusError { get; set; }

        public ITransferRequest ToTransferRequest()
        {
            return new TransferRequest
            {
                TransferId = TransferId,
                TransferStatus = TransferStatus,
                TransferStatusError = TransferStatusError
            };
        }
    }
}
