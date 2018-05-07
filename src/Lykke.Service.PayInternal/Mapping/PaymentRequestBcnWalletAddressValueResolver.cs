using AutoMapper;
using Common;
using Common.Log;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Mapping
{
    public class PaymentRequestBcnWalletAddressValueResolver : IValueResolver<IPaymentRequest, object, string>
    {
        private readonly IWalletManager _walletManager;
        private readonly ILog _log;

        public PaymentRequestBcnWalletAddressValueResolver(IWalletManager walletManager, ILog log)
        {
            _walletManager = walletManager;
            _log = log;
        }

        public string Resolve(IPaymentRequest source, object destination, string destMember, ResolutionContext context)
        {
            try
            {
                return _walletManager.ResolveBlockchainAddressAsync(source.WalletAddress, source.PaymentAssetId).GetAwaiter().GetResult();
            }
            catch (WalletNotFoundException ex)
            {
                _log.WriteErrorAsync(nameof(PaymentRequestBcnWalletAddressValueResolver), nameof(Resolve),
                    new {ex.WalletAddress}.ToJson(), ex);

                throw;
            }
        }
    }
}
