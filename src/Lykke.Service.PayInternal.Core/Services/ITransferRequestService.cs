using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ITransferRequestService
    {
        Task<ITransfer> CreateTransferAsync(ITransferRequest transferRequest);
        Task<ITransfer> CreateTransferAsync(ISourcesTransferRequest transferRequest);
        Task<ITransfer> CreateTransferAsync(ISingleSourceTransferRequest transferRequest);
        Task<ITransfer> UpdateTransferStatusAsync(ITransfer transfer);
        Task<ITransfer> UpdateTransferAsync(ITransfer transfer);
        Task<ITransferInfo> GetTransferInfoAsync(ITransfer transfer);
    }
}
