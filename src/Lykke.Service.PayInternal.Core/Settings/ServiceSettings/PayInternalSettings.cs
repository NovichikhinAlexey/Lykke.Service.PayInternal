using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.PayInternal.Core.Settings.ServiceSettings
{
    [UsedImplicitly]
    public class PayInternalSettings
    {
        public DbSettings Db { get; set; }
        public RabbitMqSettings Rabbit { get; set; }
        public ExpirationPeriodsSettings ExpirationPeriods { get; set; }
        public LpMarkupSettings LpMarkup { get; set; }
        public int TransactionConfirmationCount { get; set; }
        public BlockchainExplorerSettings LykkeBlockchainExplorer { get; set; }
        public AssetsAvailabilitySettings AssetsAvailability { get; set; }
        public BlockchainSettings Blockchain { get; set; }
    }

    public class LpMarkupSettings
    {
        public double Percent { get; set; }
        public double Pips { get; set; }
    }

    public class BlockchainExplorerSettings
    {
        public string TransactionUrl { get; set; }
    }
    public class AssetsAvailabilitySettings
    {
        public string PaymentAssets { get; set; }
        public string SettlementAssets { get; set; }
    }

    public class ExpirationPeriodsSettings
    {
        public OrderExpirationPeriodsSettings Order { get; set; }

        public TimeSpan Refund { get; set; }
    }

    public class BlockchainSettings
    {
        public BlockchainWalletAllocationSettings WalletAllocationPolicy { get; set; }

        public EthereumBlockchainSettings Ethereum { get; set; }
    }

    public class BlockchainWalletAllocationPolicy
    {
        public BlockchainType Blockchain { get; set; }

        public WalletAllocationPolicy WalletAllocationPolicy { get; set; }
    }

    public class BlockchainWalletAllocationSettings
    {
        public IList<BlockchainWalletAllocationPolicy> Policies { get; set; }
    }

    public class OrderExpirationPeriodsSettings
    {
        public TimeSpan Primary { get; set; }
        public TimeSpan Extended { get; set; }
    }

    public class EthereumBlockchainSettings
    {
        public string ApiKey { get; set; }
    }
}
