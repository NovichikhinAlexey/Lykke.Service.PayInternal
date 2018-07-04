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
        public BlockchainSettings Blockchain { get; set; }
        public JobPeriods JobPeriods { get; set; }
        public AssetPairsLocalStorageSettings AssetPairsLocalStorage { get; set; }
        public CacheSettings CacheSettings { get; set; }
        public MerchantSettings Merchant { get; set; }
        public RetryPolicySettings RetryPolicy { get; set; }
    }

    public class MerchantSettings
    {
        public int LogoSize { get; set; }
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

    public class ExpirationPeriodsSettings
    {
        public OrderExpirationPeriodsSettings Order { get; set; }

        public TimeSpan Refund { get; set; }

        /// <summary>
        /// By default wallet address has the same dueDate as payment request. 
        /// WalletExtra is an extra time to keep wallet address in lock after payment request expired in order to wait for late payments.
        /// </summary>
        public TimeSpan WalletExtra { get; set; }
    }

    public class BlockchainSettings
    {
        public BlockchainWalletAllocationSettings WalletAllocationPolicy { get; set; }
        public BitcoinSettings Bitcoin { get; set; }
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

    public class BitcoinSettings
    {
        public string Network { get; set; }
        public string ExchangeHotWalletAddress { get; set; }
        public string CashoutHotWalletAddress { get; set; }
        public BlockchainExplorerSettings BlockchainExplorer { get; set; }

    }

    public class JobPeriods
    {
        public TimeSpan PaymentRequestExpirationHandling { get; set; }
    }

    public class EthereumBlockchainSettings
    {
        public string ApiKey { get; set; }
        public string ExchangeHotWalletAddress { get; set; }
        public string CashoutHotWalletAddress { get; set; }
        public BlockchainExplorerSettings BlockchainExplorer { get; set; }
    }

    public class AssetPairSetting {
        public string BaseAssetId { get; set; }
        public string QuotingAssetId { get; set; }
        public int Accuracy { get; set; }
    }

    public class AssetPairsLocalStorageSettings
    {
        public IReadOnlyList<AssetPairSetting> AssetPairs { get; set; }
    }

    public class CacheSettings
    {
        public string RedisConfiguration { get; set; }
        public string PaymentLocksCacheKeyPattern { get; set; }
        public string CheckoutLocksCacheKeyPattern { get; set; }
    }

    public class RetryPolicySettings
    {
        public int DefaultAttempts { get; set; }
    }
}
