using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.BtcTransfer;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IBtcTransferService
    {
        Task<string> ExecuteAsync(BtcTransfer transfer);
    }
}
