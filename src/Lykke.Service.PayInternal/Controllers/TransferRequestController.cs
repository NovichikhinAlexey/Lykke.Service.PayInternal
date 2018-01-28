using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Extensions;
using Lykke.Service.PayInternal.Models;
using Lykke.Service.PayInternal.Models.PaymentRequests;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api")]
    public class TransferRequestController : Controller
    {
        private readonly ITransferRequestService _transferRequestService;
        public TransferRequestController(ITransferRequestService transferRequestService)
        {
            _transferRequestService = transferRequestService;
        }

        /// <summary>
        /// Request to transfer all money.
        /// </summary>
        /// <param name="merchantId">Merchant Id.</param>
        /// <param name="destinationAddress">Destination Address.</param>
        /// <returns>The Transfer Info.</returns>
        /// <response code="200">The Transfer Info.</response>
        /// <response code="400">Invalid model.</response>
        [HttpPost]
        [Route("merchants/{merchantId}/transfer/{destinationAddress}")]
        [SwaggerOperation("TransferRequestAll")]
        [ProducesResponseType(typeof(PaymentRequestModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> TransferRequestAllAsync(string merchantId, string destinationAddress)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            if (string.IsNullOrEmpty(merchantId))
                return BadRequest(ErrorResponse.Create("Merchant Id doesn't exist"));

            if (string.IsNullOrEmpty(destinationAddress))
                return BadRequest(ErrorResponse.Create("Destination Address doesn't exist"));

            return Ok(await _transferRequestService.CreateTransferAsync(new TransferRequestModel
            {
                DestinationAddress = destinationAddress,
                MerchantId = merchantId
            }));
        }

        /// <summary>
        /// Request to transfer specify amount.
        /// </summary>
        /// <param name="merchantId">Merchant Id.</param>
        /// <param name="destinationAddress">Destination Address.</param>
        /// <param name="amount">Amount to send</param>
        /// <returns>The Transfer Info.</returns>
        /// <response code="200">The Transfer Info.</response>
        /// <response code="400">Invalid model.</response>
        [HttpPost]
        [Route("merchants/{merchantId}/transfer/{destinationAddress}")]
        [SwaggerOperation("TransferRequestAmountAll")]
        [ProducesResponseType(typeof(PaymentRequestModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> TransferRequestAmountAsync(string merchantId, string destinationAddress, string amount)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            if (string.IsNullOrEmpty(merchantId))
                return BadRequest(ErrorResponse.Create("Merchant Id doesn't exist"));

            if (string.IsNullOrEmpty(destinationAddress))
                return BadRequest(ErrorResponse.Create("Destination Address doesn't exist"));

            if (string.IsNullOrEmpty(amount))
                return BadRequest(ErrorResponse.Create("Amount doesn't exist"));

            decimal dAmount;
            if (!decimal.TryParse(amount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out dAmount))
            {
                return BadRequest(ErrorResponse.Create("Amount is not a number"));
            }

            return Ok(await _transferRequestService.CreateTransferAsync(new TransferRequestModel
            {
                DestinationAddress = destinationAddress,
                MerchantId = merchantId,
                Amount = dAmount
            }));
        }

        /// <summary>
        /// Request to transfer from specify wallet. If Amount is 0, all money will be transfered
        /// </summary>
        /// <param name="merchantId">Merchant Id.</param>
        /// <param name="destinationAddress">Destination Address.</param>
        /// <param name="amount">Amount to send</param>
        /// <param name="sourceAddress">Source Address</param>
        /// <returns>The Transfer Info.</returns>
        /// <response code="200">The Transfer Info.</response>
        /// <response code="400">Invalid model.</response>
        [HttpPost]
        [Route("merchants/{merchantId}/transfer/{destinationAddress}")]
        [SwaggerOperation("FromAddress")]
        [ProducesResponseType(typeof(PaymentRequestModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> TransferRequestFromAddressAsync(string merchantId, string destinationAddress, string amount, [FromBody] string sourceAddress)
        {
            if(!ModelState.IsValid)
            return BadRequest(new ErrorResponse().AddErrors(ModelState));

            if (string.IsNullOrEmpty(merchantId))
                return BadRequest(ErrorResponse.Create("Merchant Id doesn't exist"));

            if (string.IsNullOrEmpty(destinationAddress))
                return BadRequest(ErrorResponse.Create("Destination Address doesn't exist"));

            if (string.IsNullOrEmpty(amount))
                return BadRequest(ErrorResponse.Create("Amount doesn't exist"));


            decimal dAmount;
            if (!decimal.TryParse(amount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out dAmount))
            {
                return BadRequest(ErrorResponse.Create("Amount is not a number"));
            }

            if (string.IsNullOrEmpty(sourceAddress))
                return BadRequest(ErrorResponse.Create("Source Address doesn't exist"));

            return Ok(await _transferRequestService.CreateTransferAsync(new TransferSingleSourceRequestModel
            {
                DestinationAddress = destinationAddress,
                MerchantId = merchantId,
                Amount = dAmount,
                SourceAddress = sourceAddress
            }));
        }

        /// <summary>
        /// Request to transfer from list of sources. If Amount is 0, all money will be transfered
        /// </summary>
        /// <param name="merchantId">Merchant Id.</param>
        /// <param name="destinationAddress">Destination Address.</param>
        /// <param name="amount">Amount to send</param>
        /// <param name="sourceAddressesList">Source Addresses List</param>
        /// <returns>The Transfer Info.</returns>
        /// <response code="200">The Transfer Info.</response>
        /// <response code="400">Invalid model.</response>
        [HttpPost]
        [Route("merchants/{merchantId}/transfer/{destinationAddress}")]
        [SwaggerOperation("TransferUpdateStatus")]
        [ProducesResponseType(typeof(PaymentRequestModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> TransferRequestFromAddressesAsync(string merchantId, string destinationAddress, string amount, [FromBody] List<string> sourceAddressesList)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            if (string.IsNullOrEmpty(merchantId))
                return BadRequest(ErrorResponse.Create("Merchant Id doesn't exist"));

            if (string.IsNullOrEmpty(destinationAddress))
                return BadRequest(ErrorResponse.Create("Destination Address doesn't exist"));

            if (string.IsNullOrEmpty(amount))
                return BadRequest(ErrorResponse.Create("Amount doesn't exist"));


            decimal dAmount;
            if (!decimal.TryParse(amount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out dAmount))
            {
                return BadRequest(ErrorResponse.Create("Amount is not a number"));
            }

            if (sourceAddressesList == null || sourceAddressesList.Count == 0 ||
                sourceAddressesList.Any(string.IsNullOrEmpty))
            {
                return BadRequest(ErrorResponse.Create("Source Addresses list is incorrect"));
            }
                

            return Ok(await _transferRequestService.CreateTransferAsync(new TransferSourcesRequestModel
            {
                DestinationAddress = destinationAddress,
                MerchantId = merchantId,
                Amount = dAmount,
                SourceAddresses = (from s in sourceAddressesList
                                   select new SourceAmountModel
                                   {
                                       SourceAddress = s,
                                       Amount = 0
                                   }).ToList()
            }));
        }

        /// <summary>
        /// Request to transfer from list of sources with amounts
        /// </summary>
        /// <param name="merchantId">Merchant Id.</param>
        /// <param name="destinationAddress">Destination Address.</param>
        /// <param name="sourceAddressAmountList">Source Addresses with Amount List</param>
        /// <returns>The Transfer Info.</returns>
        /// <response code="200">The Transfer Info.</response>
        /// <response code="400">Invalid model.</response>
        [HttpPost]
        [Route("merchants/{merchantId}/transfer/{destinationAddress}")]
        [SwaggerOperation("TransferUpdateStatus")]
        [ProducesResponseType(typeof(PaymentRequestModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> TransferRequestFromAddressesWithAmountAsync(string merchantId, string destinationAddress, [FromBody] List<SourceAmountModel> sourceAddressAmountList)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            if (string.IsNullOrEmpty(merchantId))
                return BadRequest(ErrorResponse.Create("Merchant Id doesn't exist"));

            if (string.IsNullOrEmpty(destinationAddress))
                return BadRequest(ErrorResponse.Create("Destination Address doesn't exist"));

            if (sourceAddressAmountList == null || sourceAddressAmountList.Count == 0 ||
                sourceAddressAmountList.Any(sa=> string.IsNullOrEmpty(sa.SourceAddress) || sa.Amount <= 0))
            {
                return BadRequest(ErrorResponse.Create("Source Address Amount list is incorrect"));
            }


            return Ok(await _transferRequestService.CreateTransferAsync(new TransferSourcesRequestModel
            {
                DestinationAddress = destinationAddress,
                MerchantId = merchantId,
                Amount = 0,
                SourceAddresses = sourceAddressAmountList
            }));
        }
    }
}
