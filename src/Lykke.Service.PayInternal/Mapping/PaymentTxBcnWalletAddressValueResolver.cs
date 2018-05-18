using AutoMapper;
using Common;
using Common.Log;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Mapping
{
    public class PaymentTxBcnWalletAddressValueResolver : IValueResolver<IPaymentRequestTransaction, object, string>
    {
        private readonly IWalletManager _walletManager;
        private readonly ILog _log;

        public PaymentTxBcnWalletAddressValueResolver(IWalletManager walletManager, ILog log)
        {
            _walletManager = walletManager;
            _log = log;
        }

        public string Resolve(IPaymentRequestTransaction source, object destination, string destMember,
            ResolutionContext context)
        {
            try
            {
                return _walletManager.ResolveBlockchainAddressAsync(source.WalletAddress, source.AssetId).GetAwaiter()
                    .GetResult();
            }
            catch (WalletNotFoundException ex)
            {
                _log.WriteErrorAsync(nameof(PaymentTxBcnWalletAddressValueResolver), nameof(Resolve),
                    new { ex.WalletAddress }.ToJson(), ex);

                throw;
            }
        }
    }
}
