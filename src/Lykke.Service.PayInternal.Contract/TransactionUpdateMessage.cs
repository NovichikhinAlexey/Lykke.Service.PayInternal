namespace Lykke.Service.PayInternal.Contract
{
    public class TransactionUpdateMessage
    {
        public string Id { get; set; }
        public string WalletAddresss { get; set; }
        public string OrderId { get; set; }
        public double Amount { get; set; }
        public string BlockId { get; set; }
        public int Confirmations { get; set; }
    }
}
