using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core;

namespace Lykke.Service.PayInternal.Models.Wallets
{
    public class BlockchainWalletExpiredRequest
    {
        [Required]
        public string WalletAddress { get; set; }

        [Required]
        public BlockchainType Blockchain { get; set; }
    }
}
