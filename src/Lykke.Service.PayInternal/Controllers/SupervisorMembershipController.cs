using AutoMapper;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Filters;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.SupervisorMembership;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Models.SupervisorMembership;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/supervisorMembership")]
    public class SupervisorMembershipController : Controller
    {
        private readonly ISupervisorMembershipService _supervisorMembershipService;
        private readonly IMerchantService _merchantService;
        private readonly IMerchantGroupService _merchantGroupService;
        private readonly ILog _log;

        public SupervisorMembershipController(
            [NotNull] ISupervisorMembershipService supervisorMembershipService,
            [NotNull] ILog log, 
            [NotNull] IMerchantService merchantService, 
            [NotNull] IMerchantGroupService merchantGroupService)
        {
            _supervisorMembershipService = supervisorMembershipService ?? throw new ArgumentNullException(nameof(supervisorMembershipService));
            _merchantService = merchantService ?? throw new ArgumentNullException(nameof(merchantService));
            _merchantGroupService = merchantGroupService ?? throw new ArgumentNullException(nameof(merchantGroupService));
            _log = log.CreateComponentScope(nameof(SupervisorMembershipController)) ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Creates supervisor membership
        /// </summary>
        /// <returns>Supervisor membership details</returns>
        /// <param name="request">Supervisor membership request details.</param>
        /// <response code="200">Membership details</response>
        /// <response code="400">Invalid model.</response>
        /// <response code="404">Merchant not found</response>
        [HttpPost]
        [Route("merchantGroups")]
        [SwaggerOperation("AddMembership")]
        [ProducesResponseType(typeof(SupervisorMembershipResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> Add([FromBody] AddSupervisorMembershipModel request)
        {
            IMerchant merchant = await _merchantService.GetAsync(request.MerchantId);

            if (merchant == null)
                return NotFound(ErrorResponse.Create("Merchant not found"));

            foreach (string groupId in request.MerchantGroups)
            {
                if (await _merchantGroupService.GetAsync(groupId) == null)
                    return NotFound(ErrorResponse.Create($"Merchant group [{groupId}] not found"));
            }

            try
            {
                ISupervisorMembership membership =
                    await _supervisorMembershipService.AddAsync(Mapper.Map<SupervisorMembership>(request));

                return Ok(Mapper.Map<SupervisorMembershipResponse>(membership));
            }
            catch (SupervisorMembershipAlreadyExistsException ex)
            {
                _log.WriteError(nameof(Add), new {ex.EmployeeId}, ex);

                return BadRequest(ErrorResponse.Create(ex.Message));
            }
        }

        /// <summary>
        /// Returns supervisor membership details for employee
        /// </summary>
        /// <param name="employeeId">Employee id</param>
        /// <response code="200">Supervisor membership details</response>
        [HttpGet]
        [Route("merchantGroups/{employeeId}")]
        [SwaggerOperation("GetMembership")]
        [ProducesResponseType(typeof(SupervisorMembershipResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get(string employeeId)
        {
            ISupervisorMembership membership =
                await _supervisorMembershipService.GetAsync(Uri.UnescapeDataString(employeeId));

            return Ok(Mapper.Map<SupervisorMembershipResponse>(membership));
        }

        /// <summary>
        /// Updates supervisor membership
        /// </summary>
        /// <param name="request">Supervisor membership update details</param>
        /// <response code="204">Successfully updated</response>
        /// <response code="404">Merchant or membership not found</response>
        [HttpPut]
        [Route("merchantGroups")]
        [SwaggerOperation("UpdateMembership")]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ValidateModel]
        public async Task<IActionResult> Update([FromBody] UpdateSupervisorMembershipModel request)
        {
            IMerchant merchant = await _merchantService.GetAsync(request.MerchantId);

            if (merchant == null)
                return NotFound(ErrorResponse.Create("Merchant not found"));

            foreach (string groupId in request.MerchantGroups)
            {
                if (await _merchantGroupService.GetAsync(groupId) == null)
                    return NotFound(ErrorResponse.Create($"Merchant group [{groupId}] not found"));
            }

            try
            {
                await _supervisorMembershipService.UpdateAsync(Mapper.Map<SupervisorMembership>(request));

                return NoContent();
            }
            catch (SupervisorMembershipNotFoundException ex)
            {
                _log.WriteError(nameof(Update), new { ex.EmployeeId }, ex);

                return NotFound(ErrorResponse.Create(ex.Message));
            }
        }

        /// <summary>
        /// Removes supervisor membership for employee
        /// </summary>
        /// <param name="employeeId">Employee id</param>
        /// <response code="204">Successfully removed</response>
        /// <response code="404">Membership for employee not found</response>
        [HttpDelete]
        [Route("{employeeId}")]
        [SwaggerOperation("RemoveMembership")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Remove(string employeeId)
        {
            employeeId = Uri.UnescapeDataString(employeeId);

            try
            {
                await _supervisorMembershipService.RemoveAsync(employeeId);

                return NoContent();
            }
            catch (SupervisorMembershipNotFoundException ex)
            {
                _log.WriteError(nameof(Remove), new { ex.EmployeeId }, ex);

                return NotFound(ErrorResponse.Create(ex.Message));
            }
        }

        /// <summary>
        /// Creates supervisor membership
        /// </summary>
        /// <returns>Supervisor membership details</returns>
        /// /// <response code="200">Membership details</response>
        /// <response code="400">Invalid model.</response>
        /// <response code="404">Merchant not found</response>
        [HttpPost]
        [Route("merchants")]
        [SwaggerOperation("AddMembershipForMerchants")]
        [ProducesResponseType(typeof(MerchantsSupervisorMembershipResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> AddForMerchants([FromBody] AddSupervisorMembershipMerchantsModel request)
        {
            IMerchant merchant = await _merchantService.GetAsync(request.MerchantId);

            if (merchant == null)
                return NotFound(ErrorResponse.Create("Employee merchant not found"));

            foreach (string requestMerchant in request.Merchants)
            {
                if (await _merchantService.GetAsync(requestMerchant) == null)
                    return NotFound(ErrorResponse.Create($"Merchant [{requestMerchant}] not found"));
            }

            try
            {
                IMerchantsSupervisorMembership membership =
                    await _supervisorMembershipService.AddAsync(Mapper.Map<MerchantsSupervisorMembership>(request));

                return Ok(Mapper.Map<MerchantsSupervisorMembershipResponse>(membership));
            }
            catch (SupervisorMembershipAlreadyExistsException ex)
            {
                _log.WriteError(nameof(AddForMerchants), new { ex.EmployeeId }, ex);

                return BadRequest(ErrorResponse.Create(ex.Message));
            }
        }

        /// <summary>
        /// Returns supervisor membership details with merchants for employee
        /// </summary>
        /// <param name="employeeId">Employee id</param>
        /// <response code="200">Supervisor membership details with merchants</response>
        [HttpGet]
        [Route("merchants/{employeeId}")]
        [SwaggerOperation("GetMembershipWithMerchants")]
        [ProducesResponseType(typeof(MerchantsSupervisorMembershipResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetWithMerchants(string employeeId)
        {
            IMerchantsSupervisorMembership membership =
                await _supervisorMembershipService.GetWithMerchantsAsync(Uri.UnescapeDataString(employeeId));

            return Ok(Mapper.Map<MerchantsSupervisorMembershipResponse>(membership));
        }
    }
}
