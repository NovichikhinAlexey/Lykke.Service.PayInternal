using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    public class MultipartTransferResponse
    {
        [JsonConverter(typeof(StringEnumConverter))]
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
