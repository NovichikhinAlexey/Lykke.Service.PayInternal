using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Models
{
    public class SourceAmountModel : ISourceAmount
    {
        public string SourceAddress { get; set; }
        public decimal Amount { get; set; }
    }
}
