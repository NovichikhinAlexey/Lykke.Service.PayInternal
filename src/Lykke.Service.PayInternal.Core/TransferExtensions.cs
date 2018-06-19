using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

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

        public static Task<TransferResult> ExecuteAsync(this ITransferService src, string assetId, decimal amount,
            string sourceAddress, string destAddress)
        {
            return src.ExecuteAsync(new TransferCommand
            {
                AssetId = assetId,
                Amounts = new List<TransferAmount>
                {
                    new TransferAmount
                    {
                        Amount = amount,
                        Source = sourceAddress,
                        Destination = destAddress
                    }
                }
            });
        }

        public static async Task<TransferResult> ExchangeThrowFail(this ITransferService src, string assetId,
            decimal amount, string sourceAddress, string destAddress)
        {
            TransferResult transferResult = await src.ExecuteAsync(assetId, amount, sourceAddress, destAddress);

            if (!transferResult.HasSuccess())
                throw new ExchangeOperationFailedException { TransferErrors = transferResult.GetErrors() };

            if (transferResult.HasError())
                throw new ExchangeOperationPartiallyFailedException { TransferErrors = transferResult.GetErrors() };

            return transferResult;
        }
    }
}
