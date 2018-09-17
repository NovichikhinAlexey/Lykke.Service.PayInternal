using Lykke.Service.PayInternal.Core.Domain;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class RequestMarkup : IRequestMarkup
    {
        public decimal FixedFee { get; set; }
        public decimal Percent { get; set; }
        public int Pips { get; set; }
    }
}
