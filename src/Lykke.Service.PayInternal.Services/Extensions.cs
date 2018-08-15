using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Lykke.Service.EthereumCore.Client.Models;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    public static class Extensions
    {
        public static WalletAllocationPolicy GetPolicy(this IList<BlockchainWalletAllocationPolicy> src,
            BlockchainType blockchainType)
        {
            BlockchainWalletAllocationPolicy policySetting = src.SingleOrDefault(x => x.Blockchain == blockchainType);

            return policySetting?.WalletAllocationPolicy ?? WalletAllocationPolicy.New;
        }

        public static TransactionErrorType GetDomainError(this ApiException src)
        {
            if (src?.Error == null)
                return TransactionErrorType.None;

            switch (src.Error.Code)
            {
                case ExceptionType.NotEnoughFunds:
                    return TransactionErrorType.NotEnoughFunds;
                default:
                    return TransactionErrorType.Unknown;
            }
        }

        public static TDestination Map<TSource, TDestination>(this TDestination destination, TSource source)
        {
            return Mapper.Map(source, destination);
        }
    }
}
