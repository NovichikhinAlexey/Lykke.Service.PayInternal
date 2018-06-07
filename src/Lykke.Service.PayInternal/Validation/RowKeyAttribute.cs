using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Common;

namespace Lykke.Service.PayInternal.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class RowKeyAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is ICollection<string> list)
            {
                return list.All(x => x.IsValidPartitionOrRowKey());
            }

            return value?.ToString().IsValidPartitionOrRowKey() ?? false;
        }
    }
}
