using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core;

namespace Lykke.Service.PayInternal.Models.Assets
{
    public class UpdateAssetGeneralSettingsRequest
    {
        [Required]
        public string AssetDisplayId { get; set; }

        [Required]
        public bool PaymentAvailable { get; set; }

        [Required]
        public bool SettlementAvailable { get; set; }

        [Required]
        [EnumDataType(typeof(BlockchainType), ErrorMessage = "Invalid value, possible values are: None, Bitcoin, Ethereum")]
        public BlockchainType Network { get; set; }
    }
}
