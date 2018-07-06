using System;

namespace Lykke.Service.PayInternal.Core.Domain.Confirmations
{
    public class CashoutConfirmationCommand
    {
        public string EmployeeEmail { get; set; }
        public string Asset { get; set; }
        public decimal Amount { get; set; }
        public BlockchainType Blockchain { get; set; }
        public string TransactionHash { get; set; }
        public DateTime SettlementDateTime { get; set; } 
    }
}
