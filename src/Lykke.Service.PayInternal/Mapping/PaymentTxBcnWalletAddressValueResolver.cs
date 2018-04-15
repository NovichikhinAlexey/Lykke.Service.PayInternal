using AutoMapper;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Mapping
{
    public class PaymentTxBcnWalletAddressValueResolver : IValueResolver<IPaymentRequestTransaction, object, string>
    {
        private readonly IWalletManager _walletManager;

        public PaymentTxBcnWalletAddressValueResolver(IWalletManager walletManager)
        {
            _walletManager = walletManager;
        }

        public string Resolve(IPaymentRequestTransaction source, object destination, string destMember,
            ResolutionContext context)
        {
            return _walletManager.ResolveBlockchainAddressAsync(source.WalletAddress, source.AssetId).GetAwaiter()
                .GetResult();
        }
    }
}
