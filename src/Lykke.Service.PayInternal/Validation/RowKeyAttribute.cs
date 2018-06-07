using System;
using System.ComponentModel.DataAnnotations;
using Common;

namespace Lykke.Service.PayInternal.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class RowKeyAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value?.ToString().IsValidPartitionOrRowKey() ?? false;
        }
    }
}
