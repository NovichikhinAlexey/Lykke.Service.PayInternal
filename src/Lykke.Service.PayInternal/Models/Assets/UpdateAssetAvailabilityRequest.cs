using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core.Domain;

namespace Lykke.Service.PayInternal.Models.Assets
{
    public class UpdateAssetAvailabilityRequest
    {
        [Required]
        public string AssetId { get; set; }

        [Required]
        public AssetAvailabilityType AvailabilityType { get; set; }

        [Required]
        public bool Value { get; set; }
    }
}
