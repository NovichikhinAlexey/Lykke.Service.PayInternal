using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core.Domain.Cashout;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.PayInternal.Models.Cashout;
using Swashbuckle.AspNetCore.SwaggerGen;
using Lykke.Service.PayInternal.Core;
using LykkePay.Common.Validation;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/cashout/[action]")]
    public class CashoutController : Controller
    {
        private readonly ICashoutService _cashoutService;
        private readonly ILog _log;

        public CashoutController(
            [NotNull] ICashoutService cashoutService, 
            [NotNull] ILogFactory logFactory)
        {
            _cashoutService = cashoutService ?? throw new ArgumentNullException(nameof(cashoutService));
            _log = logFactory.CreateLog(this);
        }

        /// <summary>
        /// Executes cashout operation
        /// </summary>
        /// <param name="request">Cashout request details</param>
        /// <returns></returns>
        /// <response code="200">Cashout result details</response>
        /// <response code="400">Asset network not defined, insufficient funds, cashout failed,
        /// default merchant wallet not found, merchant is not the owner of the wallet, 
        /// cashout hotwallet not defined</response>
        [HttpPost]
        [SwaggerOperation(nameof(Execute))]
        [ProducesResponseType(typeof(CashoutResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> Execute([FromBody] CashoutModel request)
        {
            try
            {
                CashoutResult cashoutResult = await _cashoutService.ExecuteAsync(Mapper.Map<CashoutCommand>(request));

                return Ok(Mapper.Map<CashoutResponse>(cashoutResult));
            }
            catch (AssetNetworkNotDefinedException e)
            {
                _log.ErrorWithDetails(e, new {e.AssetId});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (InvalidOperationException e)
            {
                _log.ErrorWithDetails(e, request);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (InsufficientFundsException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.WalletAddress,
                    e.AssetId
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (CashoutOperationFailedException e)
            {
                _log.ErrorWithDetails(e, new {errors = e.TransferErrors});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (MultipleDefaultMerchantWalletsException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.AssetId,
                    e.MerchantId,
                    e.PaymentDirection
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (DefaultMerchantWalletNotFoundException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.AssetId,
                    e.MerchantId,
                    e.PaymentDirection
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (MerchantWalletOwnershipException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.MerchantId,
                    e.WalletAddress
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (CashoutHotwalletNotDefinedException e)
            {
                _log.ErrorWithDetails(e, new {e.Blockchain});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }
    }
}
