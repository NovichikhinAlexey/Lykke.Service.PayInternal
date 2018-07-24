using System;
using Common;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    public class AutoSettleSettingsResolver : IAutoSettleSettingsResolver
    {
        private readonly AutoSettleSettings _autoSettleSettings;

        public AutoSettleSettingsResolver([NotNull] AutoSettleSettings autoSettleSettings)
        {
            _autoSettleSettings = autoSettleSettings ?? throw new ArgumentNullException(nameof(autoSettleSettings));
        }

        public bool AllowToMakePartialAutoSettle(string assetId)
        {
            return _autoSettleSettings.AssetsToMakePartialAutoSettle.Contains(assetId);
        }

        public bool AllowToSettleToMerchantWallet(string assetId)
        {
            return _autoSettleSettings.AssetsToSettleToMerchantWallet.Contains(assetId);
        }

        public string GetAutoSettleWallet(BlockchainType blockchain)
        {
            string address;

            switch (blockchain)
            {
                case BlockchainType.Bitcoin:
                    address = _autoSettleSettings.BitcoinAutoSettleWalletAddress;
                    break;
                case BlockchainType.Ethereum:
                case BlockchainType.EthereumIata:
                    address = _autoSettleSettings.EthereumAutoSettleWalletAddress;
                    break;
                case BlockchainType.None:
                    address = string.Empty;
                    break;
                default:
                    throw new Exception("Unexpected blockchain type");
            }

            return address;
        }
    }
}
