using Lykke.Service.PayInternal.Core.Domain.MerchantGroup;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IMerchantGroupService
    {
        Task<IMerchantGroup> GetAsync(string groupId);
        Task<IMerchantGroup> SetAsync(IMerchantGroup merchantGroup);
    }
}
