using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Extensions;
using Lykke.Service.PayInternal.Models.Transfers;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api")]
    public class TransfersController : Controller
    {
        private readonly ITransferService _transferService;
        private readonly ILog _log;

        public TransfersController(ITransferService transferService, ILog log)
        {
            _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Executes BTC multipartTransfer from source addresses to destination address
        /// </summary>
        /// <param name="request"></param>
        /// <returns>List of transaction ids</returns>
        [HttpPost]
        [Route("transfers/BtcFreeTransfer")]
        [SwaggerOperation("BtcFreeTransfer")]
        [ProducesResponseType(typeof(BtcTransferResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> BtcFreeTransferAsync([FromBody] BtcFreeTransferRequest request)
        {
            try
            {
                string transactionId = await _transferService.ExecuteAsync(request.ToDomain());

                return Ok(new BtcTransferResponse {TransactionId = transactionId});
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(TransfersController), nameof(BtcFreeTransferAsync), ex);

                if (ex is TransferException btcException)
                    return StatusCode((int) HttpStatusCode.InternalServerError, ErrorResponse.Create(btcException.Message));
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Request transfer from a list of some source address(es) to a list of destination address(es) with amounts specified.
        /// </summary>
        /// <param name="request">The data containing serialized model object.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("transfers/CrosswiseTransfer")]
        [SwaggerOperation("CrosswiseTransfer")]
        [ProducesResponseType(typeof(MultipartTransferResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CrosswiseTransferAsync([FromBody] CrosswiseTransferRequest request)
        {
            // Block of common validation. Note: numeric values and enums are "0" by default even if they are not presented in given request object.
            if (!ModelState.IsValid)
                return BadRequest(
                    new ErrorResponse().AddErrors(ModelState));

            if (!request.Sources.Any())
                return BadRequest(
                    ErrorResponse.Create("List of source addresses can not be empty."));

            if (!request.Destinations.Any())
                return BadRequest(
                    ErrorResponse.Create("List of destination addresses can not be empty."));

            switch (request.CheckAmountsValidity())
            {
                case TransferRequestModelValidationResult.NegativeFeeRate:
                    return BadRequest(
                        ErrorResponse.Create("The FeeRate should not be negative. The multipartTransfer is impossible."));

                case TransferRequestModelValidationResult.NegativeFixedFee:
                    return BadRequest(
                        ErrorResponse.Create("The FixedFee should not be negative. The multipartTransfer is impossible."));

                case TransferRequestModelValidationResult.NegativeSourceAmount:
                    return BadRequest(
                        ErrorResponse.Create("Some source address has the requested multipartTransfer amount <= 0. The multipartTransfer is impossible."));

                case TransferRequestModelValidationResult.NegatveDestinationAmount:
                    return BadRequest(
                        ErrorResponse.Create("Some destination address has the requested multipartTransfer amount <= 0. The multipartTransfer is impossible."));

                case TransferRequestModelValidationResult.NotEqualSourceDestAmounts:
                    return BadRequest(
                        ErrorResponse.Create("The requested sum for crosswise multipartTransfer doesn't coincide with the sum of the specified source addresses' money amounts. The multipartTransfer is impossible."));
            }
            
            // Main work
            try
            {
                // TODO: add ability to set TransactionType in request
                var result = await _transferService.ExecuteMultipartTransferAsync(request.ToDomain(), TransactionType.Refund);

                return Ok(result);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(TransfersController), nameof(CrosswiseTransferAsync), ex);

                if (ex is TransferException btcException)
                    return StatusCode((int)HttpStatusCode.InternalServerError, ErrorResponse.Create(btcException.Message));
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Request transfer consistent of a list of signle-source and single-destination transactions with amounts specified for every address pair.
        /// </summary>
        /// <param name="request">The data containing serialized model object.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("transfers/MultiBijectiveTransfer")]
        [SwaggerOperation("MultiBijectiveTransfer")]
        [ProducesResponseType(typeof(MultipartTransferResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> MultiBijectiveTransferAsync([FromBody] MultiBijectiveTransferRequest request)
        {
            // Block of common validation. Note: numeric values and enums are "0" by default even if they are not presented in given request object.
            if (!ModelState.IsValid)
                return BadRequest(
                    new ErrorResponse().AddErrors(ModelState));

            switch (request.CheckAmountsValidity())
            {
                case TransferRequestModelValidationResult.NegativeFeeRate:
                    return BadRequest(
                        ErrorResponse.Create("The FeeRate should not be negative. The multipartTransfer is impossible."));

                case TransferRequestModelValidationResult.NegativeFixedFee:
                    return BadRequest(
                        ErrorResponse.Create("The FixedFee should not be negative. The multipartTransfer is impossible."));

                case TransferRequestModelValidationResult.NegatveDestinationAmount:
                    return BadRequest(
                        ErrorResponse.Create("Some destination address has the requested multipartTransfer amount <= 0. The multipartTransfer is impossible."));
            }

            // Main work
            try
            {
                // TODO: add ability to set TransactionType in request
                var result = await _transferService.ExecuteMultipartTransferAsync(request.ToDomain(), TransactionType.Refund);

                return Ok(result);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(TransfersController), nameof(MultiBijectiveTransferAsync), ex);

                if (ex is TransferException btcException)
                    return StatusCode((int)HttpStatusCode.InternalServerError, ErrorResponse.Create(btcException.Message));
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
}
