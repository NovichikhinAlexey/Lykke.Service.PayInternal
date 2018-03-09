using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public class MultipartTransferResponse
    {
        public TransferExecutionResult State { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> TransactionIdList { get; set; }
    }

    public enum TransferExecutionResult
    {
        Success,
        SuccessInPart,
        Fail
    }
}
