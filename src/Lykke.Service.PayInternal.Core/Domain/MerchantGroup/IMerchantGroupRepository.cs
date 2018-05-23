using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.MerchantGroup
{
    public interface IMerchantGroupRepository
    {
        Task<IMerchantGroup> GetAsync(string ownerMerchantId, string groupId);
        Task<IMerchantGroup> InsertAsync(IMerchantGroup merchantGroup);
    }
}
