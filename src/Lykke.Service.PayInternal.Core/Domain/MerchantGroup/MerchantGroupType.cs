using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.MerchantGroup
{
    public enum MerchantGroupType
    {
        [JsonConverter(typeof(StringEnumConverter))]
        Supervisor
    }
}
