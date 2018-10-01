using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Mapping
{
    public class PaymentRequestBcnWalletAddressValueResolver : IValueResolver<IPaymentRequest, object, string>
    {
        private readonly IWalletManager _walletManager;
        private readonly ILog _log;

        public PaymentRequestBcnWalletAddressValueResolver(
            [NotNull] IWalletManager walletManager,
            [NotNull] ILogFactory logFactory)
        {
            _walletManager = walletManager;
            _log = logFactory.CreateLog(this);
        }

        public string Resolve(IPaymentRequest source, object destination, string destMember, ResolutionContext context)
        {
            try
            {
                return _walletManager.ResolveBlockchainAddressAsync(
                        source.WalletAddress,
                        source.PaymentAssetId)
                    .GetAwaiter().GetResult();
            }
            catch (WalletNotFoundException e)
            {
                _log.Warning(e.Message, e, $"Wallet address = {e.WalletAddress}");

                return string.Empty;
            }
        }
    }
}
