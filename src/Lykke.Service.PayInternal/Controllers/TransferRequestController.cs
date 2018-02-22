using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Common;
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
        /// Request transfer from a list of some source address(es) to a list of destination address(es) with amounts specified.
        /// </summary>
        /// <param name="requestModel">The data containing serialized model object.</param>
        /// <returns>The transfer info.</returns>
        /// <response code="200">The Transfer Info.</response>
        /// <response code="400">Invalid model (description is also provided).</response>
        [HttpPost]
        [Route("merchants/transferCrosswise")]
        [SwaggerOperation("TransferCrosswise")]
        [ProducesResponseType(typeof(ITransferRequest), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> TransferCrosswiseAsync([FromBody] TransferRequestCrosswiseModel requestModel)
        {
            // Block of common validation. Note: numeric values and enums are "0" by default even if they are not presented in given request object.
            if (!ModelState.IsValid)
                return BadRequest(
                    new ErrorResponse().AddErrors(ModelState));

            if (!requestModel.Sources.Any())
                return BadRequest(
                    ErrorResponse.Create("List of source addresses can not be empty."));

            if (!requestModel.Destinations.Any())
                return BadRequest(
                    ErrorResponse.Create("List of destination addresses can not be empty."));

            var requestValidationError = requestModel.CheckAmountsValidity();
            if (!string.IsNullOrEmpty(requestValidationError))
                return BadRequest(
                    ErrorResponse.Create(requestValidationError));

            var transferRequest = requestModel.ToTransferRequest();
            if (transferRequest == null)
                return BadRequest(
                    ErrorResponse.Create("Transfer model is malformed. Checkup list of sources and destinations."));

            // The main work
            var transferRequestResult = await _transferRequestService.CreateTransferCrosswiseAsync(transferRequest);
            if (transferRequestResult.TransferStatus == TransferStatus.Error)
                return BadRequest(
                    ErrorResponse.Create("Execution of the transfer was terminated for some reasons. Please, note, that some of its transactions may have passed with success. The transfer data is attached: " +
                        transferRequestResult.ToJson()));

            return Ok(transferRequestResult);
        }

        /// <summary>
        /// Request transfer consistent of a list of signle-source and single-destination transactions with amounts specified for every address pair.
        /// </summary>
        /// <param name="requestModel">The data containing serialized model object.</param>
        /// <returns>The transfer info.</returns>
        /// <response code="200">The Transfer Info.</response>
        /// <response code="400">Invalid model (description is also provided).</response>
        [HttpPost]
        [Route("merchants/transferMultiBijective")]
        [SwaggerOperation("TransferMultiBijective")]
        [ProducesResponseType(typeof(ITransferRequest), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> TransferMultiBijectiveAsync([FromBody] TransferRequestMultiBijectiveModel requestModel)
        {
            // Block of common validation. Note: numeric values and enums are "0" by default even if they are not presented in given request object.
            if (!ModelState.IsValid)
                return BadRequest(
                    new ErrorResponse().AddErrors(ModelState));

            var requestValidationError = requestModel.CheckAmountsValidity();
            if (!string.IsNullOrEmpty(requestValidationError))
                return BadRequest(
                    ErrorResponse.Create(requestValidationError));

            var transferRequest = requestModel.ToTransferRequest();
            if (transferRequest == null)
                return BadRequest(
                    ErrorResponse.Create("Transfer model is malformed. Checkup list of sources and destinations."));

            // The main work
            var transferRequestResult = await _transferRequestService.CreateTransferCrosswiseAsync(transferRequest);
            if (transferRequestResult.TransferStatus == TransferStatus.Error)
                return BadRequest(
                    ErrorResponse.Create("Execution of the transfer was terminated for some reasons. Please, note, that some of its transactions may have passed with success. The transfer data is attached: " +
                        transferRequestResult.ToJson()));

            return Ok(transferRequestResult);
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
        [Obsolete]
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
        [Obsolete]
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
        [Obsolete]
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
        [Obsolete]
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
                                   select new AddressAmount
                                   {
                                       Address = s,
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
        [Obsolete]
        public async Task<IActionResult> TransfersRequestFromAddressesWithAmountAsync(string merchantId, string destinationAddress, [FromBody] List<AddressAmount> sourceAddressAmountList)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

           
            if (sourceAddressAmountList == null || sourceAddressAmountList.Count == 0 ||
                sourceAddressAmountList.Any(sa=> string.IsNullOrEmpty(sa.Address) || sa.Amount <= 0))
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
