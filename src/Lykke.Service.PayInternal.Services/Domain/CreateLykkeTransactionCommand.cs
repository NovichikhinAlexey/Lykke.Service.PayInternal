using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class CreateLykkeTransactionCommand : ICreateLykkeTransactionCommand
    {
        public string OperationId { get; set; }
        public string[] SourceWalletAddresses { get; set; }
        public decimal Amount { get; set; }
        public string AssetId { get; set; }
        public int Confirmations { get; set; }
        public BlockchainType Blockchain { get; set; }
        public TransactionType Type { get; set; }
        public TransactionIdentityType IdentityType { get; set; }
        public string Identity { get; set; }
    }
}
