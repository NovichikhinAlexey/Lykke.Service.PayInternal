using Lykke.Service.PayInternal.Core.Domain;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class RequestMarkup : IRequestMarkup
    {
        public double FixedFee { get; set; }
        public double Percent { get; set; }
        public int Pips { get; set; }
    }
}
