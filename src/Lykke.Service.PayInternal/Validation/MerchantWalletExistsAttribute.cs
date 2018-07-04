using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Validation
{
    public class MerchantWalletExistsAttribute : ValidationAttribute
    {
        private const string Message = "Merchant wallet not found";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var merchantWalletService =
                (IMerchantWalletService) validationContext.GetService(typeof(IMerchantWalletService));

            if (merchantWalletService == null)
                throw new ArgumentNullException(nameof(merchantWalletService));

            string merchantWalletId = (string) value;

            if (string.IsNullOrEmpty(merchantWalletId))
                return ValidationResult.Success;

            try
            {
                merchantWalletService.GetByIdAsync(merchantWalletId).GetAwaiter().GetResult();
            }
            catch(InvalidRowKeyValueException)
            {
                return new ValidationResult(Message);
            }
            catch (MerchantWalletIdNotFoundException)
            {
                return new ValidationResult(Message);
            }

            return ValidationResult.Success;
        }
    }
}
