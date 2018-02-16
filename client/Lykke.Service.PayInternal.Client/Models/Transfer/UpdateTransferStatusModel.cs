
namespace Lykke.Service.PayInternal.Client.Models.Transfer
{
    public class UpdateTransferStatusModel
    {
        public string TransferId { get; set; }
        public string TransactionHash { get; set; }
        public TransferStatus TransferStatus { get; set; }
        public TransferStatusError TransferStatusError { get; set; }

       
    }

}
