using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Extensions;
using Lykke.Service.PayInternal.Models;
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
        [Route("merchants/{merchantId}/transfersAll/{destinationAddress}")]
        [SwaggerOperation("TransfersRequestAll")]
        [ProducesResponseType(typeof(ITransferRequest), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> TransfersRequestAllAsync(string merchantId, string destinationAddress)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            
            return Ok(await _transferRequestService.CreateTransferAsync(new TransferRequestModel
            {
                DestinationAddress = destinationAddress,
                MerchantId = merchantId
            }.ToTransferRequest()));
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
        [Route("merchants/{merchantId}/transfersAll/{destinationAddress}/amount/{amount}")]
        [SwaggerOperation("TransfersRequestAmountAll")]
        [ProducesResponseType(typeof(ITransferRequest), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> TransfersRequestAmountAsync(string merchantId, string destinationAddress, string amount)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

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
            }.ToTransferRequest()));
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
        [Route("merchants/{merchantId}/transfersFromAddress/{destinationAddress}/amount/{amount}")]
        [SwaggerOperation("TransfesRequestFromAddress")]
        [ProducesResponseType(typeof(ITransferRequest), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> TransfersRequestFromAddressAsync(string merchantId, string destinationAddress, string amount, [FromBody] string sourceAddress)
        {
            if(!ModelState.IsValid)
            return BadRequest(new ErrorResponse().AddErrors(ModelState));

           
            decimal dAmount;
            if (!decimal.TryParse(amount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out dAmount))
            {
                return BadRequest(ErrorResponse.Create("Amount is not a number"));
            }


            return Ok(await _transferRequestService.CreateTransferAsync(new TransferSingleSourceRequestModel
            {
                DestinationAddress = destinationAddress,
                MerchantId = merchantId,
                Amount = dAmount,
                SourceAddress = sourceAddress
            }.ToTransferRequest()));
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
        [Route("merchants/{merchantId}/transfersFromAddresses/{destinationAddress}/amount/{amount}")]
        [SwaggerOperation("TransfersRequestFromAddressAmount")]
        [ProducesResponseType(typeof(ITransferRequest), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> TransfersRequestFromAddressesAsync(string merchantId, string destinationAddress, string amount, [FromBody] List<string> sourceAddressesList)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

           
            decimal dAmount;
            if (!decimal.TryParse(amount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out dAmount))
            {
                return BadRequest(ErrorResponse.Create("Amount is not a number"));
            }

            if (sourceAddressesList == null || sourceAddressesList.Any() ||
                sourceAddressesList.Any(string.IsNullOrEmpty))
            {
                return BadRequest(ErrorResponse.Create("Source Addresses list is incorrect"));
            }
                

            return Ok(await _transferRequestService.CreateTransferAsync(new TransferMultipleSourcesRequestModel
            {
                DestinationAddress = destinationAddress,
                MerchantId = merchantId,
                Amount = dAmount,
                SourceAddresses = (from s in sourceAddressesList
                                   select new SourceAmount
                                   {
                                       SourceAddress = s,
                                       Amount = 0
                                   }).ToList()
            }.ToTransferRequest()));
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
        [Route("merchants/{merchantId}/transfersOnlyFromAddresses/{destinationAddress}")]
        [SwaggerOperation("TransfersOnlyFromAddress")]
        [ProducesResponseType(typeof(ITransferRequest), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> TransfersRequestFromAddressesWithAmountAsync(string merchantId, string destinationAddress, [FromBody] List<SourceAmount> sourceAddressAmountList)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

           
            if (sourceAddressAmountList == null || sourceAddressAmountList.Count == 0 ||
                sourceAddressAmountList.Any(sa=> string.IsNullOrEmpty(sa.SourceAddress) || sa.Amount <= 0))
            {
                return BadRequest(ErrorResponse.Create("Source Address Amount list is incorrect"));
            }


            return Ok(await _transferRequestService.CreateTransferAsync(new TransferMultipleSourcesRequestModel
            {
                DestinationAddress = destinationAddress,
                MerchantId = merchantId,
                Amount = 0,
                SourceAddresses = sourceAddressAmountList
            }.ToTransferRequest()));
        }
    }
}
