using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core;

namespace Lykke.Service.PayInternal.Models.Wallets
{
    public class BlockchainWalletExpiredRequest
    {
        [Required]
        public string WalletAddress { get; set; }

        [Required]
        [EnumDataType(typeof(BlockchainType), ErrorMessage = "Invalid value, possible values are: Bitcoin, Ethereum, EthereumIata")]
        public BlockchainType Blockchain { get; set; }
    }
}
