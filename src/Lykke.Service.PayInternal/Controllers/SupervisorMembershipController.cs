using AutoMapper;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core.Domain.SupervisorMembership;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Models.SupervisorMembership;
using Lykke.Service.PayInternal.Core;
using LykkePay.Common.Validation;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/supervisorMembership")]
    public class SupervisorMembershipController : Controller
    {
        private readonly ISupervisorMembershipService _supervisorMembershipService;
        private readonly ILog _log;

        public SupervisorMembershipController(
            [NotNull] ISupervisorMembershipService supervisorMembershipService,
            [NotNull] ILogFactory logFactory)
        {
            _supervisorMembershipService = supervisorMembershipService ?? throw new ArgumentNullException(nameof(supervisorMembershipService));
            _log = logFactory.CreateLog(this);
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
        [ValidateModel]
        public async Task<IActionResult> Add([FromBody] AddSupervisorMembershipModel request)
        {
            try
            {
                ISupervisorMembership membership =
                    await _supervisorMembershipService.AddAsync(Mapper.Map<SupervisorMembership>(request));

                return Ok(Mapper.Map<SupervisorMembershipResponse>(membership));
            }
            catch (SupervisorMembershipAlreadyExistsException e)
            {
                _log.ErrorWithDetails(e, new {e.EmployeeId});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Returns supervisor membership details for employee
        /// </summary>
        /// <param name="employeeId">Employee id</param>
        /// <response code="200">Supervisor membership details</response>
        /// <response code="404">Supervisor membership not found</response>
        [HttpGet]
        [Route("merchantGroups/{employeeId}")]
        [SwaggerOperation("GetMembership")]
        [ProducesResponseType(typeof(SupervisorMembershipResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get(string employeeId)
        {
            try
            {
                ISupervisorMembership membership =
                    await _supervisorMembershipService.GetAsync(Uri.UnescapeDataString(employeeId));

                return Ok(Mapper.Map<SupervisorMembershipResponse>(membership));
            }
            catch (InvalidRowKeyValueException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.Variable,
                    e.Value
                });

                return NotFound(ErrorResponse.Create("Employee not found"));
            }
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
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ValidateModel]
        public async Task<IActionResult> Update([FromBody] UpdateSupervisorMembershipModel request)
        {
            try
            {
                await _supervisorMembershipService.UpdateAsync(Mapper.Map<SupervisorMembership>(request));

                return NoContent();
            }
            catch (SupervisorMembershipNotFoundException e)
            {
                _log.ErrorWithDetails(e, new {e.EmployeeId});

                return NotFound(ErrorResponse.Create(e.Message));
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
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> Remove(string employeeId)
        {
            try
            {
                await _supervisorMembershipService.RemoveAsync(Uri.UnescapeDataString(employeeId));

                return NoContent();
            }
            catch (SupervisorMembershipNotFoundException e)
            {
                _log.ErrorWithDetails(e, new {e.EmployeeId});

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (InvalidRowKeyValueException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.Variable,
                    e.Value
                });

                return NotFound(ErrorResponse.Create("Employee not found"));
            }
        }

        /// <summary>
        /// Creates supervisor membership
        /// </summary>
        /// <returns>Supervisor membership details</returns>
        /// <response code="200">Membership details</response>
        /// <response code="400">Invalid model.</response>
        [HttpPost]
        [Route("merchants")]
        [SwaggerOperation("AddMembershipForMerchants")]
        [ProducesResponseType(typeof(MerchantsSupervisorMembershipResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> AddForMerchants([FromBody] AddSupervisorMembershipMerchantsModel request)
        {
            try
            {
                IMerchantsSupervisorMembership membership =
                    await _supervisorMembershipService.AddAsync(Mapper.Map<MerchantsSupervisorMembership>(request));

                return Ok(Mapper.Map<MerchantsSupervisorMembershipResponse>(membership));
            }
            catch (SupervisorMembershipAlreadyExistsException e)
            {
                _log.ErrorWithDetails(e, new {e.EmployeeId});

                return BadRequest(ErrorResponse.Create(e.Message));
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
        [ProducesResponseType(typeof(MerchantsSupervisorMembershipResponse), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetWithMerchants(string employeeId)
        {
            try
            {
                IMerchantsSupervisorMembership membership =
                    await _supervisorMembershipService.GetWithMerchantsAsync(Uri.UnescapeDataString(employeeId));

                return Ok(Mapper.Map<MerchantsSupervisorMembershipResponse>(membership));
            }
            catch (InvalidRowKeyValueException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.Variable,
                    e.Value
                });

                return NotFound(ErrorResponse.Create("Employee not found"));
            }
        }
    }
}
