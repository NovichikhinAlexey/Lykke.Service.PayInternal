using System;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Validation
{
    public class DateNotNullAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return (DateTime) value != DateTime.MinValue;
        }
    }
}
