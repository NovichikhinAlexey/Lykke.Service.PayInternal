using System;

namespace Lykke.Service.PayInternal.Core.Settings.ServiceSettings
{
    public class PayInternalSettings
    {
        public DbSettings Db { get; set; }
        public RabbitMqSettings Rabbit { get; set; }
        public TimeSpan OrderExpiration { get; set; }
        public LpMarkupSettings LpMarkup { get; set; }
        public int TransactionConfirmationCount { get; set; }
        public BlockchainExplorerSettings LykkeBlockchainExplorer { get; set; }
        public AssetsAvailability AssetsAvailability { get; set; }
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
    public class AssetsAvailability
    {
        public string PaymentAssets { get; set; }
        public string SettlementAssets { get; set; }
    }
}
