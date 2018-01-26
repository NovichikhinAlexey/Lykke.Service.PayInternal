using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public interface ITransferRepository
    {
        IEnumerable<ITransferInfo> GetAllAsync();
        IEnumerable<ITransferInfo> GetAsync(string transferRequestId);
        ITransferInfo GetAsync(string transferRequestId, string transactionHash);
        ITransferInfo SaveAsync(ITransferInfo transferInfo);
    }
}
