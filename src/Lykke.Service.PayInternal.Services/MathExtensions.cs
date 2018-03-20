using System;

namespace Lykke.Service.PayInternal.Services
{
    public static class MathExtensions
    {
        public static decimal GetMinValue(this int accuracy)
        {
            return (decimal) Math.Pow(10, -1 * accuracy);
        }
    }
}
