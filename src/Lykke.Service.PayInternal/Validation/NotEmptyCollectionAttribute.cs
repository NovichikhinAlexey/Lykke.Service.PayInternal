using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Validation
{
    public class NotEmptyCollectionAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is ICollection collection)
            {
                return collection.Count != 0;
            }

            return false;
        }
    }
}
