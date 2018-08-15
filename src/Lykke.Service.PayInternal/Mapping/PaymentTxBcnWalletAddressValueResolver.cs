using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Mapping
{
    public class PaymentTxBcnWalletAddressValueResolver : IValueResolver<IPaymentRequestTransaction, object, string>
    {
        private readonly IWalletManager _walletManager;
        private readonly ILog _log;

        public PaymentTxBcnWalletAddressValueResolver(
            [NotNull] IWalletManager walletManager,
            [NotNull] ILogFactory logFactory)
        {
            _walletManager = walletManager;
            _log = logFactory.CreateLog(this);
        }

        public string Resolve(IPaymentRequestTransaction source, object destination, string destMember,
            ResolutionContext context)
        {
            try
            {
                return _walletManager.ResolveBlockchainAddressAsync(
                        source.WalletAddress,
                        source.AssetId)
                    .GetAwaiter().GetResult();
            }
            catch (WalletNotFoundException e)
            {
                _log.Error(e, new {e.WalletAddress});

                throw;
            }
        }
    }
}
