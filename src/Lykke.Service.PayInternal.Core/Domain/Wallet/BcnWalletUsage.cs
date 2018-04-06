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
    }
}
