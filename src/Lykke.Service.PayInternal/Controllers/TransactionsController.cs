using System;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models;
using Lykke.Service.PayInternal.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/[controller]")]
    public class TransactionsController : Controller
    {
        private readonly ITransactionsService _transactionsService;
        private readonly ILog _log;

        public TransactionsController(
            ITransactionsService transactionsService,
            ILog log)
        {
            _transactionsService = transactionsService ?? throw new ArgumentNullException(nameof(transactionsService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Registers new transaction
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("CreateTransaction")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            try
            {
                await _transactionsService.Create(request.ToDomain());

                return Ok();
            }
            catch (Exception e)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(CreateTransaction), e);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Updates existing transaction
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("UpdateTransaction")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateTransaction([FromBody] UpdateTransactionRequest request)
        {
            try
            {
                await _transactionsService.Update(request.ToDomain());

                return Ok();
            }
            catch (Exception e)
            {
                await _log.WriteErrorAsync(nameof(TransactionsController), nameof(UpdateTransaction), e);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
