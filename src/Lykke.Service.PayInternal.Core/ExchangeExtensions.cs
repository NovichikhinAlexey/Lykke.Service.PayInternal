using System;
using Lykke.Service.PayInternal.Core.Domain.Exchange;

namespace Lykke.Service.PayInternal.Core
{
    public static class ExchangeExtensions
    {
        public static string GetWalletId(this ExchangeCommand src, PaymentDirection paymentDirection)
        {
            string walletId;

            switch (paymentDirection)
            {
                case PaymentDirection.Outgoing:
                    walletId = src.SourceMerchantWalletId;
                    break;
                case PaymentDirection.Incoming:
                    walletId = src.DestMerchantWalletId;
                    break;
                default: throw new Exception("Unexpected payment direction value");
            }

            return walletId;
        }

        public static string GetAssetId(this ExchangeCommand src, PaymentDirection paymentDirection)
        {
            string assetId;

            switch (paymentDirection)
            {
                case PaymentDirection.Outgoing:
                    assetId = src.SourceAssetId;
                    break;
                case PaymentDirection.Incoming:
                    assetId = src.DestAssetId;
                    break;
                default: throw new Exception("Unexpected payment direction value");
            }

            return assetId;
        }
    }
}
