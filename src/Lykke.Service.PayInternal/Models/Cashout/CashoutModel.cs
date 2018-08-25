using System;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Validation;
using LykkePay.Common.Validation;

namespace Lykke.Service.PayInternal.Models.Cashout
{
    /// <summary>
    /// Cashout request details
    /// </summary>
    public class CashoutModel
    {
        /// <summary>
        /// Gets ot sets merchant id
        /// </summary>
        [Required]
        [RowKey]
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets employee email
        /// </summary>
        [Required]
        [Validation.EmailAddress]
        public string EmployeeEmail { get; set; }

        /// <summary>
        /// Gets or sets source merchant wallet id
        /// </summary>
        [CanBeNull]
        [MerchantWalletExists]
        public string SourceMerchantWalletId { get; set; }

        /// <summary>
        /// Gets or sets source asset id
        /// </summary>
        [Required]
        [AssetExists]
        public string SourceAssetId { get; set; }

        /// <summary>
        /// Gets or sets desired asset
        /// </summary>
        [Required]
        public string DesiredAsset { get; set; }

        /// <summary>
        /// Gets or sets source amount
        /// </summary>
        [Required]
        [Range(Double.Epsilon, Double.MaxValue, ErrorMessage = "Source amount must be greater than 0")]
        public decimal SourceAmount { get; set; }
    }
}
