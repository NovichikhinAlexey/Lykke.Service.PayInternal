using System;
using System.Collections.Generic;
using Lykke.Service.PayInternal.Core.Domain.MerchantWallet;

namespace Lykke.Service.PayInternal.Core
{
    public static class MerchantWalletExtensions
    {
        public static IList<string> GetDefaultAssets(this IMerchantWallet src, PaymentDirection paymentDirection)
        {
            IList<string> assets;

            switch (paymentDirection)
            {
                case PaymentDirection.Incoming:
                    assets = src.IncomingPaymentDefaults;
                    break;
                case PaymentDirection.Outgoing:
                    assets = src.OutgoingPaymentDefaults;
                    break;
                default: throw new Exception($"Unexpected payment direction [{paymentDirection.ToString()}]");
            }

            return assets;
        }
    }
}
