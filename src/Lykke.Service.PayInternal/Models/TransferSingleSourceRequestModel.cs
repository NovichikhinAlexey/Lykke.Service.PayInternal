using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Models;

namespace Lykke.Service.PayInternal.Controllers
{
    public class TransferSingleSourceRequestModel : TransferRequestModel, ISingleSourceTransferRequest
    {
        public string SourceAddress { get; set; }
    }
}
