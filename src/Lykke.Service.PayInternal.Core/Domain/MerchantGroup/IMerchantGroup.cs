using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PayInternal.Core.Domain.MerchantGroup
{
    public interface IMerchantGroup
    {
        string Id { get; }
        string DisplayName { get; }
        string OwnerMerchantId { get; }
        string Merchants { get; }
        AppointmentType Appointment { get; }
    }
}
