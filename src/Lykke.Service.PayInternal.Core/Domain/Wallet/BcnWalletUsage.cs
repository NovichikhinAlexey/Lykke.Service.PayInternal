using System;

namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public class BcnWalletUsage : IBcnWalletUsage
    {
        private string _occupiedBy;

        public string WalletAddress { get; set; }

        public BlockchainType Blockchain { get; set; }

        public string OccupiedBy
        {
            get => _occupiedBy;

            set
            {
                _occupiedBy = value;
                Since = DateTime.UtcNow;
            }
        }

        public DateTime Since { get; set; }

        public static BcnWalletUsage Create(string walletAddress, BlockchainType blockchain, string occupiedBy)
        {
            return new BcnWalletUsage
            {
                WalletAddress = walletAddress,
                Blockchain = blockchain,
                OccupiedBy = occupiedBy
            };
        }

        public static BcnWalletUsage CreateVacant(string walletAddress, BlockchainType blockchain)
        {
            return Create(walletAddress, blockchain, null);
        }
    }
}
