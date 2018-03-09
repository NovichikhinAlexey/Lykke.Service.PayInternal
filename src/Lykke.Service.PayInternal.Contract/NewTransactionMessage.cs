namespace Lykke.Service.PayInternal.Contract
{
    public class NewTransactionMessage
    {
        public string Id { get; set; }
        public double Amount { get; set; }
        public string AssetId { get; set; }
        public string BlockId { get; set; }
        public string Blockchain { get; set; }
        public int Confirmations { get; set; }
    }
}
