using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Transfer;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    public interface ITransferReportApi
    {
        [Post("/api/transfers/updateStatus")]
        Task<TransferRequest> UpdateTransferStatusAsync([Body] UpdateTransferStatusModel model);
    }
}
