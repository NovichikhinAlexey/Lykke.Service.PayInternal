using Lykke.Service.PayInternal.Core.Domain.Orders;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class CalculatedAmountInfo : ICalculatedAmountInfo
    {
        public decimal PaymentAmount { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
