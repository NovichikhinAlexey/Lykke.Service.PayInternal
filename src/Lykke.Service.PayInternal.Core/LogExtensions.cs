﻿using System;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;

namespace Lykke.Service.PayInternal.Core
{
    public static class LogExtensions
    {
        public static void Error(
            [NotNull] this ILog log,
            [CanBeNull] Exception exception = null,
            [CanBeNull] object context = null)
        {
            log.Error(exception, null, $"details: {context?.ToJson()}");
        }

        public static void Error(
            [NotNull] this ILog log,
            [NotNull] string process,
            [CanBeNull] Exception exception = null,
            [CanBeNull] object context = null)
        {
            log.Error(process, exception, null, $"details: {context?.ToJson()}");
        }
    }
}