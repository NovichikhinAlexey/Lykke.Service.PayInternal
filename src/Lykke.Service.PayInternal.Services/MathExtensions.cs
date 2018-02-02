using System;

namespace Lykke.Service.PayInternal.Services
{
    public static class MathExtensions
    {
        public static decimal SatoshiToBtc(this decimal satoshi)
        {
            return satoshi * (decimal) Math.Pow(10, -1 * 8);
        }

        public static decimal GetMinValue(this int accuracy)
        {
            return (decimal) Math.Pow(10, -1 * accuracy);
        }
    }
}
