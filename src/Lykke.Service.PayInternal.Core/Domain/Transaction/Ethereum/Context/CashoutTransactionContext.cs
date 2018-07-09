namespace Lykke.Service.PayInternal.Core.Domain.Transaction.Ethereum.Context
{
    public class CashoutTransactionContext
    {
        public string EmployeeEmail { get; set; }
        public string DesiredAsset { get; set; }
        public string HistoryOperationId { get; set; }
    }
}
