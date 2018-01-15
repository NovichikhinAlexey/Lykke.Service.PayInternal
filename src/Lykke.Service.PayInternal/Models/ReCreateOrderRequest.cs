using Lykke.Service.PayInternal.Core.Domain.Order;

namespace Lykke.Service.PayInternal.Models
{
    public class ReCreateOrderRequest : IReCreateOrderRequest
    {
        public string WalletAddress { get; set; }
    }
}
