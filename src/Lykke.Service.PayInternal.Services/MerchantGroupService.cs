using Common.Log;
using Lykke.Service.PayInternal.Core.Services;
using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain.Groups;
using Lykke.Service.PayInternal.Core.Exceptions;

namespace Lykke.Service.PayInternal.Services
{
    public class MerchantGroupService : IMerchantGroupService
    {
        private readonly IMerchantGroupRepository _merchantGroupRepository;
        private readonly ILog _log;

        public MerchantGroupService(
            [NotNull] IMerchantGroupRepository merchantGroupRepository,
            [NotNull] ILog log)
        {
            _merchantGroupRepository = merchantGroupRepository ?? throw new ArgumentNullException(nameof(merchantGroupRepository)); 
            _log = log.CreateComponentScope(nameof(MerchantGroupService)) ?? throw new ArgumentNullException(nameof(log));
        }

        public Task<IMerchantGroup> GetAsync(string id)
        {
            return _merchantGroupRepository.GetAsync(id);
        }

        public Task<IMerchantGroup> CreateAsync(IMerchantGroup src)
        {
            try
            {
                return _merchantGroupRepository.CreateAsync(src);
            }
            catch (DuplicateKeyException ex)
            {
                _log.WriteError(nameof(CreateAsync), src, ex);

                throw new MerchantGroupAlreadyExistsException(src.DisplayName);
            }
        }
    }
}
