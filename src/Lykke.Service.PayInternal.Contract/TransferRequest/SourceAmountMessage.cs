namespace Lykke.Service.PayInternal.Contract.TransferRequest
{
    public class SourceAmountMessage
    {
        public string SourceAddress { get; set; }
        public decimal Amount { get; set; }
    }
}
