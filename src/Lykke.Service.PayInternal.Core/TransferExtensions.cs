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

        public static Task<TransferResult> ExecuteAsync(this ITransferService src, string assetId,
            string sourceAddress, string destAddress, decimal? amount = null)
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
            string sourceAddress, string destAddress, decimal? amount = null)
        {
            TransferResult transferResult = await src.ExecuteAsync(assetId, sourceAddress, destAddress, amount);

            if (!transferResult.HasSuccess())
                throw new ExchangeOperationFailedException { TransferErrors = transferResult.GetErrors() };

            if (transferResult.HasError())
                throw new ExchangeOperationPartiallyFailedException { TransferErrors = transferResult.GetErrors() };

            return transferResult;
        }

        public static async Task<TransferResult> SettleThrowFail(this ITransferService src, string assetId,
            string sourceAddress, string destAddress, decimal? amount = null)
        {
            TransferResult transferResult = await src.ExecuteAsync(assetId, sourceAddress, destAddress, amount);

            if (!transferResult.HasSuccess())
                throw new SettlementOperationFailedException {TransferErrors = transferResult.GetErrors()};

            if (transferResult.HasError())
                throw new SettlementOperationPartiallyFailedException {TransferErrors = transferResult.GetErrors()};

            return transferResult;
        }

        public static async Task<TransferResult> PayThrowFail(this ITransferService src, string assetId,
            string sourceAddress, string destAddress, decimal? amount = null)
        {
            TransferResult transferResult = await src.ExecuteAsync(assetId, sourceAddress, destAddress, amount);

            if (!transferResult.HasSuccess())
                throw new PaymentOperationFailedException {TransferErrors = transferResult.GetErrors()};

            if (transferResult.HasError())
                throw new PaymentOperationPartiallyFailedException {TransferErrors = transferResult.GetErrors()};

            return transferResult;
        }
    }
}
