﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Extensions;
using Lykke.Service.PayInternal.Models;
using Lykke.Service.PayInternal.Services.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api")]
    public class MerchantsController : Controller
    {
        private readonly IMerchantService _merchantService;
        private readonly ILog _log;

        public MerchantsController(
            IMerchantService merchantService,
            ILog log)
        {
            _merchantService = merchantService;
            _log = log;
        }

        /// <summary>
        /// Returns all merchants.
        /// </summary>
        /// <returns>The collection of merchants.</returns>
        /// <response code="200">The collection of merchants.</response>
        [HttpGet]
        [Route("merchants")]
        [SwaggerOperation("MerchantsGetAll")]
        [ProducesResponseType(typeof(List<MerchantModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAsync()
        {
            IReadOnlyList<IMerchant> merchants = await _merchantService.GetAsync();

            var model = Mapper.Map<List<MerchantModel>>(merchants);

            return Ok(model);
        }
        
        /// <summary>
        /// Returns merchant.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <returns>The merchant.</returns>
        /// <response code="200">The merchant.</response>
        /// <response code="404">The merchant not found.</response>
        [HttpGet]
        [Route("merchants/{merchantId}")]
        [SwaggerOperation("MerchantsGetById")]
        [ProducesResponseType(typeof(MerchantModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAsync(string merchantId)
        {
            IMerchant merchant = await _merchantService.GetAsync(merchantId);

            if (merchant == null)
                return NotFound();
            
            var model = Mapper.Map<MerchantModel>(merchant);

            return Ok(model);
        }
        
        /// <summary>
        /// Creates merchant.
        /// </summary>
        /// <returns>The created merchant.</returns>
        /// <param name="request">The merchant create request.</param>
        /// <response code="200">The created merchant.</response>
        /// <response code="400">Invalid model.</response>
        [HttpPost]
        [Route("merchants")]
        [SwaggerOperation("MerchantsCreate")]
        [ProducesResponseType(typeof(MerchantModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateAsync([FromBody] CreateMerchantRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            try
            {
                var merchant = Mapper.Map<Merchant>(request);

                IMerchant createdMerchant = await _merchantService.CreateAsync(merchant);

                return Ok(Mapper.Map<MerchantModel>(createdMerchant));
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(MerchantsController), nameof(CreateAsync), request.ToJson(), exception);
                throw;
            }
        }
 
        /// <summary>
        /// Updates a merchant.
        /// </summary>
        /// <param name="request">The merchant update request.</param>
        /// <response code="204">The merchant successfully updated.</response>
        /// <response code="400">Invalid model.</response>
        /// <response code="404">The merchant not found.</response>
        [HttpPatch]
        [Route("merchants")] // TODO: merchants/{merchantId} when Refit can use path parameter and body togather
        [SwaggerOperation("MerchantsUpdate")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateMerchantRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            try
            {
                var merchant = Mapper.Map<Merchant>(request);

                await _merchantService.UpdateAsync(merchant);
            }
            catch (MerchantNotFoundException exception)
            {
                await _log.WriteWarningAsync(nameof(MerchantsController), nameof(UpdateAsync),
                    request.ToJson(), exception.Message);
                return NotFound();
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(MerchantsController), nameof(UpdateAsync), request.ToJson(), exception);
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Sets merchant public key.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="file">The public key file.</param>
        /// <response code="204">The public key successfully updated.</response>
        /// <response code="404">The merchant not found.</response>
        [HttpPost]
        [Route("merchants/{merchantId}/publickey")]
        [SwaggerOperation("MerchantsSetPublicKey")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> SetPublicKeyAsync(string merchantId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ErrorResponse.Create("Empty file"));

            try
            {
                var fileContent = await file.OpenReadStream().ToBytesAsync();
                string publicKey = Encoding.UTF8.GetString(fileContent, 0, fileContent.Length);

                await _merchantService.SetPublicKeyAsync(merchantId, publicKey);

                return NoContent();
            }
            catch (MerchantNotFoundException exception)
            {
                await _log.WriteWarningAsync(nameof(MerchantsController), nameof(SetPublicKeyAsync),
                    new {MerchantId = merchantId}.ToJson(), exception.Message);
                return NotFound();
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(MerchantsController), nameof(SetPublicKeyAsync),
                    new {MerchantId = merchantId}.ToJson(), exception);

                throw;
            }
        }
        
        /// <summary>
        /// Deletes a merchant.
        /// </summary>
        /// <param name="merchantId">The merchan id.</param>
        /// <response code="204">Merchant successfully deleted.</response>
        [HttpDelete]
        [Route("merchants/{merchantId}")]
        [SwaggerOperation("MerchantsDelete")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteAsync(string merchantId)
        {
            await _merchantService.DeleteAsync(merchantId);
            
            return NoContent();
        }
    }
}