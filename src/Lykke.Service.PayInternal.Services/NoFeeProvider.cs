using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class NoFeeProvider : IFeeProvider
    {
        public int FeeRate => 0;

        public decimal FixedFee => 0;
    }
}
