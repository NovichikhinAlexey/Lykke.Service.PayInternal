using System;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.Wallet;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public static class ConvertExtensions
    {
        public static CreateWallet ToDomain(this ICreateWalletRequest src)
        {
            return new CreateWallet
            {
                DueDate = src.DueDate,
                MerchantId = src.MerchantId
            };
        }

        public static CreateOrder ToDomain(this ICreateOrderRequest src)
        {
            return new CreateOrder
            {
                MerchantId = src.MerchantId,
                ExchangeAssetId = src.ExchangeAssetId,
                InvoiceAmount = src.InvoiceAmount,
                InvoiceAssetId = src.InvoiceAssetId,
                AssetPairId = src.AssetPairId,
                MarkupPips = src.MarkupPips,
                MarkupPercent = src.MarkupPercent,
                InvoiceId = src.InvoiceId,
                WalletDueDate = src.WalletDueDate,
                WalletAddress = null
            };
        }

        public static ReCreateOrder ToDomain(this IReCreateOrderRequest src)
        {
            return new ReCreateOrder
            {
                WalletAddress = src.WalletAddress
            };
        }

        public static CreateWallet ToWalletDomain(this ICreateOrder src, DateTime orderDueDate)
        {
            return new CreateWallet
            {
                DueDate = src.WalletDueDate ?? orderDueDate,
                MerchantId = src.MerchantId
            };
        }
    }
}
