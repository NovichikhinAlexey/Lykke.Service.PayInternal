using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public interface ITransferRepository
    {
        Task AddAsync(IMultipartTransfer transfer);
    }
}
