using Common.Log;
using Lykke.Service.PayInternal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.AzureRepositories;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Groups;
using Lykke.Service.PayInternal.Core.Exceptions;
using KeyNotFoundException = Lykke.Service.PayInternal.Core.Exceptions.KeyNotFoundException;

namespace Lykke.Service.PayInternal.Services
{
    public class MerchantGroupService : IMerchantGroupService
    {
        private readonly IMerchantGroupRepository _merchantGroupRepository;
        private readonly ILog _log;

        public MerchantGroupService(
            [NotNull] IMerchantGroupRepository merchantGroupRepository,
            [NotNull] ILogFactory logFactory)
        {
            _merchantGroupRepository = merchantGroupRepository ??
                                       throw new ArgumentNullException(nameof(merchantGroupRepository));
            _log = logFactory.CreateLog(this);
        }

        public Task<IMerchantGroup> CreateAsync(IMerchantGroup src)
        {
            try
            {
                return _merchantGroupRepository.CreateAsync(src);
            }
            catch (DuplicateKeyException ex)
            {
                _log.Error(ex, src);

                throw new MerchantGroupAlreadyExistsException(src.DisplayName);
            }
        }

        public Task<IMerchantGroup> GetAsync(string id)
        {
            return _merchantGroupRepository.GetAsync(id);
        }

        public async Task UpdateAsync(IMerchantGroup src)
        {
            try
            {
                await _merchantGroupRepository.UpdateAsync(src);
            }
            catch (KeyNotFoundException ex)
            {
                _log.Error(ex, src);

                throw new MerchantGroupNotFoundException(src.Id);
            }
        }

        public async Task DeleteAsync(string id)
        {
            try
            {
                await _merchantGroupRepository.DeleteAsync(id);
            }
            catch (KeyNotFoundException ex)
            {
                _log.Error(ex, new {merchantGroupId = id});

                throw new MerchantGroupNotFoundException(id);
            }
        }

        public async Task<IReadOnlyList<string>> GetMerchantsByUsageAsync(string merchantId,
            MerchantGroupUse merchantGroupUse)
        {
            IReadOnlyList<IMerchantGroup> groups = await _merchantGroupRepository.GetByOwnerAsync(merchantId);

            return groups
                .Where(x => x.MerchantGroupUse == merchantGroupUse)
                .SelectMany(x => x.Merchants?.Split(Constants.Separator))
                .Distinct()
                .ToList();
        }

        public Task<IReadOnlyList<IMerchantGroup>> GetByOwnerAsync(string ownerId)
        {
            return _merchantGroupRepository.GetByOwnerAsync(ownerId);
        }
    }
}
