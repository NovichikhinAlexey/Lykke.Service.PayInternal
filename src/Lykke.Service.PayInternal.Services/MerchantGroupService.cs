using Common.Log;
using Lykke.Service.PayInternal.Core.Domain.MerchantGroup;
using Lykke.Service.PayInternal.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Services
{
    public class MerchantGroupService : IMerchantGroupService
    {
        private readonly IMerchantGroupRepository _merchantGroupRepository;
        private readonly ILog _log;

        public MerchantGroupService(
            IMerchantGroupRepository merchantGroupRepository,
            ILog log)
        {
            _merchantGroupRepository = merchantGroupRepository;
            _log = log;
        }

        public Task<IMerchantGroup> GetAsync(string groupId)
        {
            return _merchantGroupRepository.GetAsync(groupId);
        }
        public Task<IMerchantGroup> SetAsync(IMerchantGroup merchantGroup)
        {
            return _merchantGroupRepository.InsertAsync(merchantGroup);
        }
    }
}
