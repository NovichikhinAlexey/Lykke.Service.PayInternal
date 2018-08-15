using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Models.Transactions
{
    public class CreateTransactionRequest : ICreateTransactionRequest
    {
        public string Hash { get; set; }

        [Required]
        public string WalletAddress { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string AssetId { get; set; }

        [Required]
        public int Confirmations { get; set; }

        public string BlockId { get; set; }

        [Required]
        [EnumDataType(typeof(BlockchainType), ErrorMessage = "Invalid value, possible values are: None, Bitcoin, Ethereum")]
        public BlockchainType Blockchain { get; set; }

        public DateTime? FirstSeen { get; set; }

        [Required]
        [EnumDataType(typeof(TransactionIdentityType), ErrorMessage = "Invalid value, possible values are: None, Hash, Specific")]
        public TransactionIdentityType IdentityType { get; set; }

        [Required]
        public string Identity { get; set; }

        [Required]
        public string[] SourceWalletAddresses { get; set; }
    }
}
