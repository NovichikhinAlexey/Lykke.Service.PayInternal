using Lykke.Service.PayInternal.Core.Domain.Order;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class ReCreateOrder : IReCreateOrder
    {
        public string WalletAddress { get; set; }
    }
}
