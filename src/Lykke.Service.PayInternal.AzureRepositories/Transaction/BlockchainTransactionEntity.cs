using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.PayInternal.AzureRepositories.Transaction
{
    public class BlockchainTransactionEntity : TableEntity, IBlockchainTransaction
    {
        public static class ByWallet
        {
            public static string GeneratePartitionKey(string walletAddress)
            {
                return walletAddress;
            }

            public static string GenerateRowKey(string txId)
            {
                return txId;
            }

            public static BlockchainTransactionEntity Create(IBlockchainTransaction src)
            {
                return new BlockchainTransactionEntity
                {
                    PartitionKey = GeneratePartitionKey(src.WalletAddress),
                    RowKey = GenerateRowKey(src.Id),
                    WalletAddress = src.WalletAddress,
                    Amount = src.Amount,
                    BlockId = src.BlockId,
                    Confirmations = src.Confirmations,
                    OrderId = src.OrderId,
                    Status = src.Status
                };
            }
        }

        public string Id => RowKey;
        public string OrderId { get; set; }
        public double Amount { get; set; }
        public string BlockId { get; set; }
        public int Confirmations { get; set; }
        public BlockchainTransactionStatus Status { get; set; }
        public string WalletAddress { get; set; }
    }
}
