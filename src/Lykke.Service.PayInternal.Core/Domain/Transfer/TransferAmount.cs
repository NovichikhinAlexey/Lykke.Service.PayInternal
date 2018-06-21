namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public class TransferAmount
    {
        public string Source { get; set; }

        public string Destination { get; set; }

        public decimal? Amount { get; set; }
    }
}
