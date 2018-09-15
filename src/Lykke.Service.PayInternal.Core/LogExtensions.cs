using System;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;

namespace Lykke.Service.PayInternal.Core
{
    public static class LogExtensions
    {
        public static string ToDetails(this object src)
        {
            return $"details: {src?.ToJson()}";
        }

        public static void ErrorWithDetails(
            [NotNull] this ILog log,
            [CanBeNull] Exception exception = null,
            [CanBeNull] object context = null)
        {
            log.Error(exception, context.ToDetails());
        }

        public static void ErrorWithDetails(
            [NotNull] this ILog log,
            [NotNull] string process,
            [CanBeNull] Exception exception = null,
            [CanBeNull] object context = null)
        {
            log.Error(process, exception, context.ToDetails());
        }
    }
}
