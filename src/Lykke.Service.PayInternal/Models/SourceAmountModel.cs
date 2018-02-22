using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Models
{
    public class SourceAmountModel : IAddressAmount
    {
        public string Address { get; set; }
        public decimal Amount { get; set; }
    }
}
