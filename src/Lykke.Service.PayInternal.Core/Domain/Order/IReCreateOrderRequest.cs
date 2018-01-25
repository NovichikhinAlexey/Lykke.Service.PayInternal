using System;

namespace Lykke.Service.PayInternal.Core.Domain.Order
{
    public interface IReCreateOrderRequest
    {
        string WalletAddress { get; set; }
    }
}
