using System.Collections.Generic;
using System.Linq;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Core
{
    public static class TransferExtensions
    {
        public static IEnumerable<TransferTransactionResult> GetFailedTxs(this TransferResult src)
        {
            return src.Transactions.Where(x => x.HasError);
        }

        public static IEnumerable<TransferTransactionResult> GetSuccedeedTxs(this TransferResult src)
        {
            return src.Transactions.Where(x => !x.HasError);
        }

        public static bool HasError(this TransferResult src)
        {
            return src.Transactions.Any(x => x.HasError);
        }

        public static bool HasSuccess(this TransferResult src)
        {
            return src.Transactions.Any(x => !x.HasError);
        }

        public static IEnumerable<string> GetErrors(this TransferResult src)
        {
            return src.GetFailedTxs().Select(x => x.Error);
        }
    }
}
