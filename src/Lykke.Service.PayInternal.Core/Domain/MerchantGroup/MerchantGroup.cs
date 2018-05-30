using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.MerchantGroup
{
    public class MerchantGroup : IMerchantGroup
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string OwnerId { get; set; }

        public string Merchants { get; set; }

        public GroupType Type { get; set; }
    }
}
