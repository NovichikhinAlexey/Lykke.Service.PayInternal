using System;
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
        public TimeSpan Order { get; set; }
        public TimeSpan Refund { get; set; }
    }
}
