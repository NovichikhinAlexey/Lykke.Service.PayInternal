using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Validation
{
    public class MerchantExistsAttribute : ValidationAttribute
    {
        private const string Message = "Merchant not found";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var merchantService = (IMerchantService) validationContext.GetService(typeof(IMerchantService));

            var merchantId = (string) value;

            if (string.IsNullOrEmpty(merchantId))
                return ValidationResult.Success;

            try
            {
                IMerchant merchant = merchantService?.GetAsync(merchantId).GetAwaiter().GetResult();

                if (merchant == null)
                    return new ValidationResult(Message);
            }
            catch (InvalidRowKeyValueException)
            {
                return new ValidationResult(Message);
            }

            return ValidationResult.Success;
        }
    }
}
