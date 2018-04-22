using System;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using NBitcoin;

namespace Lykke.Service.PayInternal.Services
{
    public class BlockchainAddressValidator : IBlockchainAddressValidator
    {
        private readonly Network _bitcoinNetwork;

        public BlockchainAddressValidator(string bitcoinNetwork)
        {
            _bitcoinNetwork = Network.GetNetwork(bitcoinNetwork);
        }

        public bool Execute(string address, BlockchainType blockchain)
        {
            switch (blockchain)
            {
                case BlockchainType.Bitcoin:
                    try
                    {
                        _bitcoinNetwork.Parse(address);
                    }
                    catch (Exception)
                    {
                        return false;
                    }

                    return true;

                case BlockchainType.Ethereum:
                    throw new NotImplementedException();

                default:
                    throw new UnrecognizedBlockchainTypeException(blockchain);
            }
        }
    }
}
