using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models.Transfers;
using LykkePay.Common.Validation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/transfers")]
    public class TransfersController : Controller
    {
        private readonly IBtcTransferService _btcTransferService;
        private readonly IDepositValidationService _depositValidationService;
        private readonly ILog _log;

        public TransfersController(
            [NotNull] IBtcTransferService btcTransferService, 
            [NotNull] ILogFactory logFactory, 
            [NotNull] IDepositValidationService depositValidationService)
        {
            _btcTransferService = btcTransferService ?? throw new ArgumentNullException(nameof(btcTransferService));
            _depositValidationService = depositValidationService ?? throw new ArgumentNullException(nameof(depositValidationService));
            _log = logFactory.CreateLog(this);
        }

        /// <summary>
        /// Executes BTC multipartTransfer from source addresses to destination address
        /// </summary>
        /// <param name="request"></param>
        /// <returns>List of transaction ids</returns>
        [HttpPost]
        [Route("btcFreeTransfer")]
        [SwaggerOperation("BtcFreeTransfer")]
        [ProducesResponseType(typeof(BtcTransferResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BtcTransferResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> BtcFreeTransferAsync([FromBody] BtcFreeTransferRequest request)
        {
            try
            {
                var result = await _btcTransferService.ExecuteAsync(Mapper.Map<BtcTransfer>(request));

                return Ok(Mapper.Map<BtcTransferResponse>(result));
            }
            catch (TransferException e)
            {
                _log.ErrorWithDetails(e, new {e.Code});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Validates money transfer from temporary deposit addresses
        /// </summary>
        /// <param name="request">Validation request details</param>
        /// <response code="200">Validation result</response>
        /// <response code="400">Blockchain type not supported or wallet address is not associated with virtual wallet</response>
        /// <response code="404">Payment request not found</response>
        [HttpGet]
        [Route("depositTransfer/validate")]
        [SwaggerOperation("ValidateDepositTransfer")]
        [ProducesResponseType(typeof(ValidateDepositTransferResult), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> ValidateDepositTransferAsync([FromQuery] ValidateDepositTransferRequest request)
        {
            try
            {
                var command =
                    Mapper.Map<ValidateDepositTransferCommand>(
                        Mapper.Map<ValidateDepositRawTransferCommand>(request));

                bool isSuccess = await _depositValidationService.ValidateDepositTransferAsync(command);

                return Ok(new ValidateDepositTransferResult {IsSuccess = isSuccess});
            }
            catch (PaymentRequestNotFoundException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.PaymentRequestId,
                    e.MerchantId,
                    e.WalletAddress
                });

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (AutoMapperMappingException e)
            {
                if (e.InnerException is BilBlockchainTypeNotSupported bcnEx)
                {
                    _log.ErrorWithDetails(bcnEx, new {bcnEx.Blockchain});

                    return BadRequest(ErrorResponse.Create(bcnEx.Message));
                }

                if (e.InnerException is BlockchainWalletNotLinkedException walletEx)
                {
                    _log.ErrorWithDetails(walletEx, new {walletEx.Blockchain, walletEx.WalletAddress});

                    return BadRequest(ErrorResponse.Create(walletEx.Message));
                }

                throw;
            }
        }
    }
}
