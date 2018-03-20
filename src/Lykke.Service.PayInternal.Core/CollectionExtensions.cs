using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.PayInternal.Core
{
    public static class CollectionExtensions
    {
        public static bool MoreThan<T>(this IEnumerable<T> src, int count)
        {
            return src.Skip(count).Any();
        }

        public static bool MoreThanOne<T>(this IEnumerable<T> src)
        {
            return src.MoreThan(1);
        }

        public static IEnumerable<string> Unique<T>(this IEnumerable<T> src, Func<T, string> selector)
        {
            return src.GroupBy(selector).Select(g => g.Key);
        }
    }
}
