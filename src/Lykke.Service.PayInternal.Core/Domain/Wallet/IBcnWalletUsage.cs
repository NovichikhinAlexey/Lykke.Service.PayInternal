using System;

namespace Lykke.Service.PayInternal.Core.Domain.Wallet
{
    public interface IBcnWalletUsage
    {
        string WalletAddress { get; set; }

        BlockchainType Blockchain { get; set; }

        string OccupiedBy { get; set; }

        DateTime Since { get; set; }
    }
}
