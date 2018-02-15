using System;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Validation
{
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value != null && (DateTime) value > DateTime.UtcNow;
        }
    }
}
