using AutoMapper;
using Lykke.Service.PayInternal.Contract.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.Asset;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Orders;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Models;
using Lykke.Service.PayInternal.Models.Assets;
using Lykke.Service.PayInternal.Models.Markups;
using Lykke.Service.PayInternal.Models.Orders;
using Lykke.Service.PayInternal.Models.PaymentRequests;
using Lykke.Service.PayInternal.Models.Transfers;
using Lykke.Service.PayInternal.Services.Domain;

namespace Lykke.Service.PayInternal.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<IMerchant, MerchantModel>(MemberList.Source);

            CreateMap<CreateMerchantRequest, Merchant>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PublicKey, opt => opt.Ignore());

            CreateMap<UpdateMerchantRequest, Merchant>(MemberList.Destination)
                .ForMember(dest => dest.PublicKey, opt => opt.Ignore());

            CreateMap<IOrder, OrderModel>(MemberList.Source);

            CreateMap<IAssetMerchantSettings, AssetMerchantSettingsResponse>();

            CreateMap<BtcTransferSourceInfo, AddressAmount>(MemberList.Destination);

            CreateMap<BtcFreeTransferRequest, BtcTransfer>(MemberList.Destination)
                .ForMember(dest => dest.FeeRate, opt => opt.MapFrom(x => 0))
                .ForMember(dest => dest.FixedFee, opt => opt.MapFrom(x => 0));

            CreateMap<IMarkup, MarkupResponse>(MemberList.Destination);

            CreateMap<IAssetGeneralSettings, AssetGeneralSettingsResponseModel>(MemberList.Destination)
                .ForMember(dest => dest.AssetDisplayId, opt => opt.MapFrom(src => src.AssetId));

            CreateMap<UpdateAssetGeneralSettingsRequest, AssetGeneralSettings>(MemberList.Destination)
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetDisplayId));

            PaymentRequestApiModels();
            PaymentRequestMessages();
        }

        private void PaymentRequestApiModels()
        {
            CreateMap<IPaymentRequest, PaymentRequestModel>(MemberList.Source)
                .ForSourceMember(src => src.OrderId, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.ExternalOrderId))
                .ForMember(dest => dest.WalletAddress, opt => opt.ResolveUsing<PaymentRequestBcnWalletAddressValueResolver>());

            CreateMap<CreatePaymentRequestModel, PaymentRequest>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.PaidAmount, opt => opt.Ignore())
                .ForMember(dest => dest.PaidDate, opt => opt.Ignore())
                .ForMember(dest => dest.ProcessingError, opt => opt.Ignore())
                .ForMember(dest => dest.Timestamp, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalOrderId, opt => opt.MapFrom(src => src.OrderId));

            CreateMap<IPaymentRequest, PaymentRequestDetailsModel>(MemberList.Source)
                .ForSourceMember(src => src.OrderId, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.ExternalOrderId))
                .ForMember(dest => dest.WalletAddress, opt => opt.ResolveUsing<PaymentRequestBcnWalletAddressValueResolver>());

            CreateMap<IOrder, PaymentRequestOrderModel>(MemberList.Source)
                .ForSourceMember(src => src.MerchantId, opt => opt.Ignore())
                .ForSourceMember(src => src.PaymentRequestId, opt => opt.Ignore())
                .ForSourceMember(src => src.AssetPairId, opt => opt.Ignore())
                .ForSourceMember(src => src.SettlementAmount, opt => opt.Ignore())
                .ForSourceMember(src => src.LwOperationId, opt => opt.Ignore());

            CreateMap<IPaymentRequestTransaction, PaymentRequestTransactionModel>(MemberList.Source)
                .ForSourceMember(src => src.Id, opt => opt.Ignore())
                .ForSourceMember(src => src.PaymentRequestId, opt => opt.Ignore())
                .ForSourceMember(src => src.WalletAddress, opt => opt.Ignore())
                .ForSourceMember(src => src.Blockchain, opt => opt.Ignore())
                .ForSourceMember(src => src.TransactionType, opt => opt.Ignore())
                .ForSourceMember(src => src.DueDate, opt => opt.Ignore())
                .ForSourceMember(src => src.TransferId, opt => opt.Ignore())
                .ForSourceMember(src => src.CreatedOn, opt => opt.Ignore())
                .ForSourceMember(src => src.IdentityType, opt => opt.Ignore())
                .ForSourceMember(src => src.Identity, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TransactionId))
                .ForMember(dest => dest.Url, opt => opt.ResolveUsing<PaymentTxUrlValueResolver>())
                .ForMember(dest => dest.RefundUrl, opt => opt.Ignore());

            CreateMap<IPaymentRequestTransaction, PayTransactionStateResponse>(MemberList.Destination)
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<PaymentTxBcnWalletAddressValueResolver>());

            CreateMap<IWalletState, WalletStateResponse>(MemberList.Destination);

            CreateMap<RefundTransactionResult, RefundTransactionReponseModel>();

            CreateMap<RefundResult, RefundResponseModel>(MemberList.Source)
                .ForSourceMember(src => src.PaymentRequestWalletAddress, opt => opt.Ignore());

            CreateMap<Core.Domain.PaymentRequests.PaymentRequestRefundTransaction, PaymentRequestRefundTransactionModel>(MemberList.Source)
                .ForMember(dest => dest.Url, opt => opt.ResolveUsing<RefundTxUrlValueResolver>())
                // todo: add the field and allow mapping
                .ForSourceMember(src => src.Blockchain, opt => opt.Ignore());

            CreateMap<Core.Domain.PaymentRequests.PaymentRequestRefund, PaymentRequestRefundModel>(MemberList.Source);
        }

        private void PaymentRequestMessages()
        {
            CreateMap<IPaymentRequest, PaymentRequestDetailsMessage>(MemberList.Source)
                .ForMember(dest => dest.Order, opt => opt.Ignore())
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<PaymentRequestBcnWalletAddressValueResolver>())
                .ForSourceMember(src => src.ExternalOrderId, opt => opt.Ignore());

            CreateMap<IOrder, PaymentRequestOrder>(MemberList.Source)
                .ForSourceMember(src => src.MerchantId, opt => opt.Ignore())
                .ForSourceMember(src => src.PaymentRequestId, opt => opt.Ignore())
                .ForSourceMember(src => src.AssetPairId, opt => opt.Ignore())
                .ForSourceMember(src => src.SettlementAmount, opt => opt.Ignore())
                .ForSourceMember(src => src.LwOperationId, opt => opt.Ignore());

            CreateMap<IPaymentRequestTransaction, Contract.PaymentRequest.PaymentRequestTransaction>(MemberList.Source)
                .ForSourceMember(src => src.Id, opt => opt.Ignore())
                .ForSourceMember(src => src.PaymentRequestId, opt => opt.Ignore())
                .ForSourceMember(src => src.WalletAddress, opt => opt.Ignore())
                .ForSourceMember(src => src.AssetId, opt => opt.Ignore())
                .ForSourceMember(src => src.Blockchain, opt => opt.Ignore())
                .ForSourceMember(src => src.TransactionType, opt => opt.Ignore())
                .ForSourceMember(src => src.DueDate, opt => opt.Ignore())
                .ForSourceMember(src => src.TransferId, opt => opt.Ignore())
                .ForSourceMember(src => src.CreatedOn, opt => opt.Ignore())
                .ForSourceMember(src => src.IdentityType, opt => opt.Ignore())
                .ForSourceMember(src => src.Identity, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TransactionId))
                .ForMember(dest => dest.Url, opt => opt.ResolveUsing<PaymentTxUrlValueResolver>());

            CreateMap<Core.Domain.PaymentRequests.PaymentRequestRefundTransaction, Contract.PaymentRequest.PaymentRequestRefundTransaction>(MemberList.Destination)
                .ForMember(dest => dest.Url, opt => opt.ResolveUsing<RefundTxUrlValueResolver>())
                // todo: add the field and allow mapping
                .ForSourceMember(src => src.Blockchain, opt => opt.Ignore());

            CreateMap<Core.Domain.PaymentRequests.PaymentRequestRefund, Contract.PaymentRequest.PaymentRequestRefund>();
        }
    }
}
