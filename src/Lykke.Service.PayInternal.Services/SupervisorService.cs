using Lykke.Service.PayInternal.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.PayInternal.Core.Domain.Supervisor;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.PayInternal.Core.Domain.MerchantGroup;
using System.Linq;

namespace Lykke.Service.PayInternal.Services
{
    public class SupervisorService : ISupervisorService
    {
        const string Delimiter = ";";
        private readonly ILog _log;
        private readonly ISupervisorRepository _supervisorRepository;
        private readonly IMerchantGroupService _merchantGroupService;
        public SupervisorService(
            IMerchantGroupService merchantGroupService,
            ISupervisorRepository supervisorRepository,
            ILog log)
        {
            _merchantGroupService = merchantGroupService;
            _supervisorRepository = supervisorRepository;
            _log = log;
        }
        public async Task<ISupervisor> GetAsync(string merchantId, string employeeId)
        {
            var supervisor = await _supervisorRepository.GetAsync(merchantId, employeeId);
            if (supervisor != null)
            {
                var groups = supervisor.MerchantGroups.Split(Delimiter).ToList();
                var allmerchants = new List<string>();
                foreach (var group in groups)
                {
                    var merchantgroup = await _merchantGroupService.GetAsync(merchantId, group);
                    allmerchants.AddRange(merchantgroup.Merchants.Split(Delimiter).ToList());
                }
                allmerchants = allmerchants.Distinct().ToList();
                supervisor.SupervisorMerchants = String.Join(Delimiter, allmerchants.ToArray());
                return supervisor;
            }
            return null;
        }

        public async Task<ISupervisor> SetAsync(ISupervisor supervisor)
        {
            var supervisorentity = await _supervisorRepository.GetAsync(supervisor.MerchantId, supervisor.EmployeeId);
            if (supervisorentity == null)
            {
                supervisorentity = await _supervisorRepository.InsertAsync(supervisor);
                var merchantGroup = new MerchantGroup();
                merchantGroup.Type = GroupType.Supervisor;
                merchantGroup.OwnerId = supervisor.MerchantId;
                merchantGroup.Merchants = supervisor.SupervisorMerchants;
                var group = await _merchantGroupService.SetAsync(merchantGroup);
                supervisorentity.MerchantGroups = group.Id;
                supervisor.MerchantGroups = group.Id;
                await _supervisorRepository.UpdateAsync(supervisor);
            }
            else
            {
                var merchantgroups = supervisorentity.MerchantGroups.Split(Delimiter).ToList();
                var modelgroups = supervisor.MerchantGroups.Split(Delimiter).ToList();
                merchantgroups.AddRange(modelgroups);
                merchantgroups = merchantgroups.Distinct().ToList();
                supervisorentity.MerchantGroups = String.Join(Delimiter, merchantgroups.ToArray());
                supervisor.MerchantGroups = String.Join(Delimiter, merchantgroups.ToArray());
                await _supervisorRepository.UpdateAsync(supervisor);
            }
            return supervisorentity;
        }
        public async Task DeleteAsync(string merchantId, string employeeId)
        {
            await _supervisorRepository.DeleteAsync(merchantId, employeeId);
        }
    }
}
