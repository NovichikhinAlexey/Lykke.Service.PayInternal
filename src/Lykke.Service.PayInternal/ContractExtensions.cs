using System.Linq;
using Lykke.Service.PayInternal.Core.Domain.BtcTransfer;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Models;

namespace Lykke.Service.PayInternal
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

        public static TransactionStateResponse ToApiModel(this IBlockchainTransaction src)
        {
            return new TransactionStateResponse
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

        public static SourceInfo ToDomain(this BtcTransferSourceInfo src)
        {
            return new SourceInfo
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
