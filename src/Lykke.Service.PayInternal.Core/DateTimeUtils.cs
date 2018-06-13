using System;

namespace Lykke.Service.PayInternal.Core
{
    public static class DateTimeUtils
    {
        public static DateTime Largest(DateTime d1, DateTime d2)
        {
            return new DateTime(Math.Max(d1.Ticks, d2.Ticks));
        }

        public static DateTime Smallest(DateTime d1, DateTime d2)
        {
            return new DateTime(Math.Min(d1.Ticks, d2.Ticks));
        }
    }
}
