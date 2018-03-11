using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Services
{
    public static class TransactionExtensions
    {
        public static DateTime GetLatestDate(this IEnumerable<IBlockchainTransaction> src)
        {
            return src.Max(x => x.FirstSeen ?? DateTime.MinValue);
        }

        public static decimal GetTotal(this IEnumerable<IBlockchainTransaction> src)
        {
            return src.Sum(x => x.Amount);
        }

        public static string GetAssetId(this IEnumerable<IBlockchainTransaction> src)
        {
            return src.Select(x => x.AssetId).FirstOrDefault();
        }
    }
}
