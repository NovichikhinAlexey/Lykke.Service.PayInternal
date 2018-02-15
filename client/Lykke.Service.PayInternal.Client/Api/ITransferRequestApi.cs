using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Client.Models.Transfer;
using Refit;

namespace Lykke.Service.PayInternal.Client.Api
{
    public interface ITransferRequestApi
    {
        [Post("/api/merchants/{merchantId}/transfersAll/{destinationAddress}")]
        Task<TransferRequest> TransfersRequestAllAsync(string merchantId, string destinationAddress);
        [Post("/api/merchants/{merchantId}/transfersAll/{destinationAddress}/amount/{amount}")]
        Task <TransferRequest> TransfersRequestAmountAsync(string merchantId, string destinationAddress, string amount);
        [Post("/api/merchants/{merchantId}/transfersFromAddress/{destinationAddress}/amount/{amount}")]
        Task<TransferRequest> TransfersRequestFromAddressAsync(string merchantId, string destinationAddress, string amount, [Body] string sourceAddress);
        [Post("/api/merchants/{merchantId}/transfersFromAddresses/{destinationAddress}/amount/{amount}")]
        Task<TransferRequest> TransfersRequestFromAddressesAsync(string merchantId, string destinationAddress, string amount, [Body] List<string> sourceAddressesList);
        [Post("/api/merchants/{merchantId}/transfersOnlyFromAddresses/{destinationAddress}")]
        Task<TransferRequest> TransfersRequestFromAddressesWithAmountAsync(string merchantId, string destinationAddress, [Body] List<SourceAmount> sourceAddressAmountList);
    }
}
