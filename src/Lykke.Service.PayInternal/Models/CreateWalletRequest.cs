using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Validation;

namespace Lykke.Service.PayInternal.Models
{
    public class CreateWalletRequest : ICreateWalletRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "MerchantId can't be empty")]
        public string MerchantId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DateNotNull(ErrorMessage = "DueDate can't be empty")]
        [FutureDate(ErrorMessage = "DueDate has to be in the future")]
        public DateTime DueDate { get; set; }
    }
}
