using System.Collections.Generic;
using System.Linq;
using Lykke.Service.PayInternal.Core.Domain.Asset;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Models;
using Lykke.Service.PayInternal.Models.Transfers;

namespace Lykke.Service.PayInternal.Extensions
{
    public static class ContractExtensions
    {
        public static WalletStateResponse ToApiModel(this IWalletState src)
        {
            return new WalletStateResponse
            {
                DueDate = src.DueDate,
                Address = src.Address,
                Transactions = src.Transactions.Select(x => x.ToApiModel())
            };
        }

        public static PayTransactionStateResponse ToApiModel(this IPaymentRequestTransaction src)
        {
            return new PayTransactionStateResponse
            {
                Id = src.Id,
                WalletAddress = src.WalletAddress,
                Amount = (double)src.Amount,
                Confirmations = src.Confirmations,
                BlockId = src.BlockId,
                AssetId = src.AssetId,
                Blockchain = src.Blockchain
            };
        }

        public static AddressAmount ToDomain(this BtcTransferSourceInfo src)
        {
            return new AddressAmount
            {
                Amount = src.Amount,
                Address = src.Address
            };
        }

        public static BtcTransfer ToDomain(this BtcFreeTransferRequest src)
        {
            return new BtcTransfer
            {
                DestAddress = src.DestAddress,
                Sources = src.Sources?.Select(x => x?.ToDomain()),
                FeeRate = 0,
                FixedFee = 0
            };
        }
    }
}
