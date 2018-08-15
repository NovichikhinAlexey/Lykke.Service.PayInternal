using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Markup;

namespace Lykke.Service.PayInternal.Models.Markups
{
    public class UpdateMarkupRequest : IMarkupValue
    {
        [Required]
        public decimal DeltaSpread { get; set; }

        [Required]
        public decimal Percent { get; set; }

        [Required]
        public int Pips { get; set; }

        [Required]
        public decimal FixedFee { get; set; }

        public string PriceAssetPairId { get; set; }

        [EnumDataType(typeof(PriceMethod), ErrorMessage = "Invalid value, possible values are: None, Direct, Reverse")]
        public PriceMethod PriceMethod { get; set; }
    }
}
