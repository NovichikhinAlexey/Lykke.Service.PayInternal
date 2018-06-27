using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Validation;

namespace Lykke.Service.PayInternal.Models.Exchange
{
    /// <summary>
    /// Exchange operation details
    /// </summary>
    public class ExchangeModel: PreExchangeModel
    {
        /// <summary>
        /// Gets or sets source merchant wallet id
        /// </summary>
        [CanBeNull]
        [MerchantWalletExists]
        public string SourceMerchantWalletId { get; set; }

        /// <summary>
        /// Gets or sets destination merchant walletd id
        /// </summary>
        [CanBeNull]
        [MerchantWalletExists]
        public string DestMerchantWalletId { get; set; }

        /// <summary>
        /// Gets or sets expected rate
        /// </summary>
        public decimal ExpectedRate { get; set; }
    }
}
