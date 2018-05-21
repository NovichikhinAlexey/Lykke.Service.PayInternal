using System;
using Common;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    public class BcnExplorerResolver : IBcnExplorerResolver
    {
        private readonly BlockchainSettings _blockchainSettings;

        public BcnExplorerResolver([NotNull] BlockchainSettings blockchainSettings)
        {
            _blockchainSettings = blockchainSettings ?? throw new ArgumentNullException(nameof(blockchainSettings));
        }

        public string GetExplorerUrl(BlockchainType blockchain, string transactionHash)
        {
            Uri uri = null;

            switch (blockchain)
            {
                case BlockchainType.Bitcoin:
                    uri = new Uri(
                        new Uri(
                            _blockchainSettings.Bitcoin.BlockchainExplorer.TransactionUrl
                                .AddLastSymbolIfNotExists('/')), transactionHash);
                    break;
                case BlockchainType.Ethereum:
                    uri = new Uri(
                        new Uri(
                            _blockchainSettings.Ethereum.BlockchainExplorer.TransactionUrl
                                .AddLastSymbolIfNotExists('/')), transactionHash);
                    break;
                case BlockchainType.None:
                    break;
                default:
                    throw new Exception("Unexpected blockchain type");
            }

            return uri?.ToString() ?? string.Empty;
        }
    }
}
