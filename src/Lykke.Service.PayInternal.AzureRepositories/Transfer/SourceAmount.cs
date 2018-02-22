using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.AzureRepositories.Transfer
{
    public class SourceAmount : IAddressAmount
    {
        public string Address { get; set; }
        public decimal Amount { get; set; }
    }
}
