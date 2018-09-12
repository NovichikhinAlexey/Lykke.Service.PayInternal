using Lykke.Service.PayInternal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core.Domain.SupervisorMembership;
using Lykke.Service.PayInternal.Core.Exceptions;
using KeyNotFoundException = Lykke.Service.PayInternal.Core.Exceptions.KeyNotFoundException;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayMerchant.Client;
using Lykke.Service.PayMerchant.Client.Models;

namespace Lykke.Service.PayInternal.Services
{
    public class SupervisorMembershipService : ISupervisorMembershipService
    {
        private readonly ISupervisorMembershipRepository _supervisorMembershipRepository;
        private readonly IPayMerchantClient _payMerchantClient;
        private readonly ILog _log;
        
        public SupervisorMembershipService(
            [NotNull] ISupervisorMembershipRepository supervisorMembershipRepository,
            [NotNull] ILogFactory logFactory, 
            [NotNull] IPayMerchantClient payMerchantClient)
        {
            _supervisorMembershipRepository = supervisorMembershipRepository ?? throw new ArgumentNullException(nameof(supervisorMembershipRepository));
            _payMerchantClient = payMerchantClient ?? throw new ArgumentNullException(nameof(payMerchantClient));
            _log = logFactory.CreateLog(this);
        }

        public async Task<IMerchantsSupervisorMembership> AddAsync(IMerchantsSupervisorMembership src)
        {
            MerchantGroupResponse merchantGroup =
                await _payMerchantClient.Groups.AddGroupAsync(Mapper.Map<AddMerchantGroupRequest>(src));

            try
            {
                ISupervisorMembership membership = await _supervisorMembershipRepository.AddAsync(
                    new SupervisorMembership
                    {
                        MerchantId = src.MerchantId,
                        EmployeeId = src.EmployeeId,
                        MerchantGroups = new[] {merchantGroup.Id}
                    });

                return new MerchantsSupervisorMembership
                {
                    MerchantId = membership.MerchantId,
                    EmployeeId = membership.EmployeeId,
                    Merchants = merchantGroup.Merchants
                };
            }
            catch (DuplicateKeyException ex)
            {
                _log.Error(ex, src);

                if (merchantGroup != null)
                    await _payMerchantClient.Groups.DeleteGroupAsync(merchantGroup.Id);

                throw new SupervisorMembershipAlreadyExistsException(src.EmployeeId);
            }
        }

        public Task<ISupervisorMembership> GetAsync(string employeeId)
        {
            return _supervisorMembershipRepository.GetAsync(employeeId);
        }

        public async Task<IMerchantsSupervisorMembership> GetWithMerchantsAsync(string employeeId)
        {
            ISupervisorMembership membership = await _supervisorMembershipRepository.GetAsync(employeeId);

            if (membership != null)
            {
                var merchants = new List<string>();

                foreach (string merchantGroupId in membership.MerchantGroups)
                {
                    MerchantGroupResponse merchantGroup = await _payMerchantClient.Groups.GetGroupAsync(merchantGroupId);

                    if (merchantGroup != null)
                        merchants.AddRange(merchantGroup.Merchants);
                }

                return new MerchantsSupervisorMembership
                {
                    MerchantId = membership.MerchantId,
                    EmployeeId = membership.EmployeeId,
                    Merchants = merchants.Distinct()
                };
            }

            return null;
        }

        public async Task UpdateAsync(ISupervisorMembership src)
        {
            try
            {
                await _supervisorMembershipRepository.UpdateAsync(src);
            }
            catch (KeyNotFoundException ex)
            {
                _log.Error(ex, src);

                throw new SupervisorMembershipNotFoundException(src.EmployeeId);
            }
        }

        public async Task<ISupervisorMembership> AddAsync(ISupervisorMembership src)
        {
            try
            {
                return await _supervisorMembershipRepository.AddAsync(src);
            }
            catch (DuplicateKeyException ex)
            {
                _log.Error(ex, src);

                throw new SupervisorMembershipAlreadyExistsException(src.EmployeeId);
            }
        }

        public async Task RemoveAsync(string employeeId)
        {
            try
            {
                await _supervisorMembershipRepository.RemoveAsync(employeeId);
            }
            catch (KeyNotFoundException ex)
            {
                _log.Error(ex, new {employeeId});

                throw new SupervisorMembershipNotFoundException(employeeId);
            }
        }
    }
}
