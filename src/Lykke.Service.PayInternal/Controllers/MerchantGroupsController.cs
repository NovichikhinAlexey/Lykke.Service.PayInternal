using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Groups;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models.MerchantGroups;
using LykkePay.Common.Validation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/merchantGroups")]
    public class MerchantGroupsController : Controller
    {
        private readonly IMerchantGroupService _merchantGroupService;
        private readonly IMerchantService _merchantService;
        private readonly ILog _log;

        public MerchantGroupsController(
            [NotNull] IMerchantGroupService merchantGroupService,
            [NotNull] IMerchantService merchantService,
            [NotNull] ILogFactory logFactory)
        {
            _merchantGroupService =
                merchantGroupService ?? throw new ArgumentNullException(nameof(merchantGroupService));
            _merchantService = merchantService ?? throw new ArgumentNullException(nameof(merchantService));
            _log = logFactory.CreateLog(this);
        }

        /// <summary>
        /// Creates merchant group
        /// </summary>
        /// <param name="request">Merchant group creation details</param>
        /// <returns>Merchant group details</returns>
        /// <response code="200">Merchant group details</response>
        /// <response code="400">Invalid model.</response>
        /// <response code="404">Merchant not found</response>
        [HttpPost]
        [SwaggerOperation("AddGroup")]
        [ProducesResponseType(typeof(MerchantGroupResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> Add([FromBody] AddMerchantGroupModel request)
        {
            foreach (string requestMerchant in request.Merchants)
            {
                if (await _merchantService.GetAsync(requestMerchant) == null)
                    return NotFound(ErrorResponse.Create($"Merchant [{requestMerchant}] not found"));
            }

            try
            {
                IMerchantGroup group = await _merchantGroupService.CreateAsync(Mapper.Map<MerchantGroup>(request));

                return Ok(Mapper.Map<MerchantGroupResponse>(group));
            }
            catch (MerchantGroupAlreadyExistsException e)
            {
                _log.Error(e, request);

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Returns merchant group details
        /// </summary>
        /// <param name="id">Group id</param>
        /// <response code="200">Merchant group details</response>
        /// <response code="404">Merchant group not found</response>
        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation("GetGroup")]
        [ProducesResponseType(typeof(MerchantGroupResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                IMerchantGroup group = await _merchantGroupService.GetAsync(Uri.UnescapeDataString(id));

                return Ok(Mapper.Map<MerchantGroupResponse>(group));
            }
            catch (InvalidRowKeyValueException e)
            {
                _log.Error(e, new
                {
                    e.Variable,
                    e.Value
                });

                return NotFound(ErrorResponse.Create("Group not found"));
            }
        }

        /// <summary>
        /// Updates merchant group
        /// </summary>
        /// <param name="request">Merchant group update details</param>
        /// <response code="204">Successfully updated</response>
        /// <response code="404">Merchant group or merchant not found</response>
        [HttpPut]
        [SwaggerOperation("UpdateGroup")]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ValidateModel]
        public async Task<IActionResult> Update([FromBody] UpdateMerchantGroupModel request)
        {
            foreach (string requestMerchant in request.Merchants)
            {
                if (await _merchantService.GetAsync(requestMerchant) == null)
                    return NotFound(ErrorResponse.Create($"Merchant [{requestMerchant}] not found"));
            }

            try
            {
                await _merchantGroupService.UpdateAsync(Mapper.Map<MerchantGroup>(request));

                return NoContent();
            }
            catch (MerchantGroupNotFoundException e)
            {
                _log.Error(e, new {e.MerchantGroupId});

                return NotFound(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Deletes merchant group
        /// </summary>
        /// <param name="id">Group id</param>
        /// <response code="204">Successfully deleted</response>
        /// <response code="404">Merchant group not found</response>
        [HttpDelete]
        [Route("{id}")]
        [SwaggerOperation("DeleteGroup")]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _merchantGroupService.DeleteAsync(Uri.UnescapeDataString(id));

                return NoContent();
            }
            catch (MerchantGroupNotFoundException e)
            {
                _log.Error(e, new {e.MerchantGroupId});

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (InvalidRowKeyValueException e)
            {
                _log.Error(e, new
                {
                    e.Variable,
                    e.Value
                });

                return NotFound(ErrorResponse.Create("Merchant group not found"));
            }
        }

        /// <summary>
        /// Returns list of merchants participating in groups by usage for particular owner (merchant provided)
        /// </summary>
        /// <param name="request">Get Merchants by usage request details</param>
        /// <response code="200">List of merchants</response>
        [HttpPost]
        [Route("merchants/byUsage")]
        [SwaggerOperation(nameof(GetMerchantsByUsage))]
        [ProducesResponseType(typeof(MerchantsByUsageResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> GetMerchantsByUsage([FromBody] GetMerchantsByUsageModel request)
        {
            if (!request.MerchantGroupUse.HasValue)
                return BadRequest(ErrorResponse.Create("MerchantGroupUse can't be empty"));

            IReadOnlyList<string> merchants =
                await _merchantGroupService.GetMerchantsByUsageAsync(request.MerchantId,
                    request.MerchantGroupUse.Value);

            return Ok(new MerchantsByUsageResponse {Merchants = merchants});
        }

        /// <summary>
        /// Returns list of groups where the given id is an owner
        /// </summary>
        /// <param name="ownerId">Owner id</param>
        /// <response code="200">List of groups</response>
        /// <response code="404">Owner not found</response>
        [HttpGet]
        [Route("byOwner/{ownerId}")]
        [SwaggerOperation("GetMerchantGroupsByOwner")]
        [ProducesResponseType(typeof(IEnumerable<MerchantGroupResponse>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetByOwner(string ownerId)
        {
            ownerId = Uri.UnescapeDataString(ownerId);

            if (!ownerId.IsValidPartitionOrRowKey())
                return NotFound(ErrorResponse.Create("Owner not found"));

            IReadOnlyList<IMerchantGroup> groups = await _merchantGroupService.GetByOwnerAsync(ownerId);

            return Ok(Mapper.Map<IEnumerable<MerchantGroupResponse>>(groups));
        }
    }
}
