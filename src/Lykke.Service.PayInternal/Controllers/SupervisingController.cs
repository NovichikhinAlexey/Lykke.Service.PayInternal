using AutoMapper;
using Common;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Domain.Supervisor;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Extensions;
using Lykke.Service.PayInternal.Filters;
using Lykke.Service.PayInternal.Models.Supervising;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/supervising")]
    public class SupervisingController : Controller
    {
        private readonly ILog _log;
        private readonly ISupervisorService _supervisorService;
        public SupervisingController(
            ILog log,
            ISupervisorService supervisorService)
        {
            _supervisorService = supervisorService;
            _log = log.CreateComponentScope(nameof(SupervisingController));
        }
        /// <summary>
        /// Returns list of merchants for Employee supervising
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{employeeId}")]
        [SwaggerOperation("GetMerchants")]
        [ProducesResponseType(typeof(AvailableMerchantsResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> GetMerchants(string employeeId)
        {
            var supervisor = await _supervisorService.GetAsync(employeeId);
            if (supervisor == null)
                return NotFound(ErrorResponse.Create("Couldn't find supervisor"));
            var list = new List<string>();
            if (supervisor.SupervisorMerchants != null)
                list = supervisor.SupervisorMerchants.Split(';').ToList();
            return Ok(new AvailableMerchantsResponseModel { Merchants = list });
        }
        /// <summary>
        /// Creates employee supervisor
        /// </summary>
        /// <returns>The created merchant.</returns>
        /// <param name="request">The merchant create request.</param>
        /// <response code="200">The created merchant.</response>
        /// <response code="400">Invalid model.</response>
        [HttpPost]
        [SwaggerOperation("EmployeeSupervisorCreate")]
        [ProducesResponseType(typeof(SupervisingModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> CreateAsync([FromBody] CreateSupervisingEmployeeRequest request)
        {
            try
            {
                var model = Mapper.Map<Supervisor>(request);
                ISupervisor supervisor = await _supervisorService.SetAsync(model);
                return Ok(Mapper.Map<SupervisingModel>(supervisor));
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(SupervisingController), nameof(CreateAsync), ex);
                throw;
            }
        }

        /// <summary>
        /// Deletes employee supervising.
        /// </summary>
        /// <param name="employeeId">The employee id.</param>
        /// <response code="204">successfully deleted.</response>
        [HttpDelete]
        [Route("{employeeId}")]
        [SwaggerOperation("SupervisingDelete")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> DeleteAsync(string employeeId)
        {
            await _supervisorService.DeleteAsync(employeeId);
            return NoContent();
        }
    }
}
