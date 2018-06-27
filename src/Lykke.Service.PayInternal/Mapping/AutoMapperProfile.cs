using System;
using AutoMapper;
using AzureStorage.Tables.Templates;
using Lykke.Service.PayInternal.AzureRepositories;
using Lykke.Service.PayInternal.Contract.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain;
using Lykke.Service.PayInternal.Core.Domain.Asset;
using Lykke.Service.PayInternal.Core.Domain.Groups;
using Lykke.Service.PayInternal.Core.Domain.AssetPair;
using Lykke.Service.PayInternal.Core.Domain.Exchange;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Orders;
using Lykke.Service.PayInternal.Core.Domain.MerchantWallet;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.SupervisorMembership;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Models;
using Lykke.Service.PayInternal.Models.AssetRates;
using Lykke.Service.PayInternal.Models.Assets;
using Lykke.Service.PayInternal.Models.Exchange;
using Lykke.Service.PayInternal.Models.Markups;
using Lykke.Service.PayInternal.Models.MerchantGroups;
using Lykke.Service.PayInternal.Models.MerchantWallets;
using Lykke.Service.PayInternal.Models.Orders;
using Lykke.Service.PayInternal.Models.PaymentRequests;
using Lykke.Service.PayInternal.Models.SupervisorMembership;
using Lykke.Service.PayInternal.Models.Transactions.Ethereum;
using Lykke.Service.PayInternal.Models.Transfers;
using Lykke.Service.PayInternal.Services.Domain;
using Lykke.Service.PayInternal.Services.Mapping;

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

            CreateMap<AddSupervisorMembershipModel, SupervisorMembership>(MemberList.Destination);

            CreateMap<UpdateSupervisorMembershipModel, SupervisorMembership>(MemberList.Destination);

            CreateMap<AddSupervisorMembershipMerchantsModel, MerchantsSupervisorMembership>(MemberList.Destination);

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

            CreateMap<ISupervisorMembership, SupervisorMembershipResponse>(MemberList.Destination);

            CreateMap<IMerchantsSupervisorMembership, MerchantsSupervisorMembershipResponse>(MemberList.Destination);

            CreateMap<AddMerchantGroupModel, MerchantGroup>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Merchants, opt => opt.MapFrom(src => string.Join(Constants.Separator, src.Merchants)));

            CreateMap<IMerchantGroup, MerchantGroupResponse>(MemberList.Destination)
                .ForMember(dest => dest.Merchants, opt => opt.MapFrom(src => src.Merchants.Split(Constants.Separator, StringSplitOptions.None)));

            CreateMap<UpdateMerchantGroupModel, MerchantGroup>(MemberList.Destination)
                .ForMember(dest => dest.Merchants, opt => opt.MapFrom(src => string.Join(Constants.Separator, src.Merchants)))
                .ForMember(dest => dest.OwnerId, opt => opt.Ignore());

            CreateMap<CreateMerchantWalletModel, CreateMerchantWalletCommand>(MemberList.Source);

            CreateMap<IMerchantWallet, MerchantWalletResponse>(MemberList.Source);

            CreateMap<MerchantWalletBalanceLine, MerchantWalletBalanceResponse>(MemberList.Destination)
                .ForMember(dest => dest.MerchantWalletId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AssetDisplayId, opt => opt.ResolveUsing<AssetDisplayIdValueResolver, string>(src => src.AssetId));

            CreateMap<PaymentModel, PaymentCommand>(MemberList.Destination);

            CreateMap<AddAssetRateModel, AddAssetPairRateCommand>(MemberList.Destination)
                .ForMember(dest => dest.BaseAssetId,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.BaseAssetId = (string) resContext.Items["BaseAssetId"]))
                .ForMember(dest => dest.QuotingAssetId,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.QuotingAssetId = (string) resContext.Items["QuotingAssetId"]));

            CreateMap<IAssetPairRate, AssetRateResponse>()
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.CreatedOn))
                .ForMember(dest => dest.BaseAssetId,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.BaseAssetId = (string) resContext.Items["BaseAssetId"]))
                .ForMember(dest => dest.QuotingAssetId,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.QuotingAssetId = (string) resContext.Items["QuotingAssetId"]));

            CreateMap<ExchangeModel, ExchangeCommand>(MemberList.Destination)
                .ForMember(dest => dest.SourceAssetId, opt => opt.ResolveUsing<AssetIdValueResolver, string>(src => src.SourceAssetId))
                .ForMember(dest => dest.DestAssetId, opt => opt.ResolveUsing<AssetIdValueResolver, string>(src => src.DestAssetId));

            CreateMap<PreExchangeModel, PreExchangeCommand>(MemberList.Destination)
                .ForMember(dest => dest.SourceAssetId, opt => opt.ResolveUsing<AssetIdValueResolver, string>(src => src.SourceAssetId))
                .ForMember(dest => dest.DestAssetId, opt => opt.ResolveUsing<AssetIdValueResolver, string>(src => src.DestAssetId));

            CreateMap<ExchangeResult, ExchangeResponse>(MemberList.Destination)
                .ForMember(dest => dest.DestAssetId,
                    opt => opt.ResolveUsing<AssetDisplayIdValueResolver, string>(src => src.DestAssetId))
                .ForMember(dest => dest.SourceAssetId,
                    opt => opt.ResolveUsing<AssetDisplayIdValueResolver, string>(src => src.SourceAssetId));

            // incoming ethereum payment
            CreateMap<RegisterInboundTxRequest, CreateTransactionCommand>(MemberList.Destination)
                .ForMember(dest => dest.DueDate, opt => opt.Ignore())
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.ToAddress))
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int) resContext.Items["Confirmations"]))
                .ForMember(dest => dest.SourceWalletAddresses, opt => opt.MapFrom(src => new[] {src.FromAddress}))
                .ForMember(dest => dest.TransferId, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.UseValue(TransactionType.Payment));

            // incoming ethereum payment update
            CreateMap<RegisterInboundTxRequest, UpdateTransactionCommand>(MemberList.Destination)
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.ToAddress))
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int) resContext.Items["Confirmations"]));

            // incoming bitcoin payment
            CreateMap<ICreateTransactionRequest, CreateTransactionCommand>(MemberList.Destination)
                .ForMember(dest => dest.DueDate, opt => opt.Ignore())
                .ForMember(dest => dest.TransferId, opt => opt.Ignore())
                .ForMember(dest => dest.Type,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Type = (TransactionType) resContext.Items["TransactionType"]))
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.WalletAddress));

            // outgoing ethereum payment update
            CreateMap<RegisterOutboundTxRequest, UpdateTransactionCommand>(MemberList.Destination)
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.ToAddress))
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int)resContext.Items["Confirmations"]));

            // outgoing ethereum payment complete
            CreateMap<CompleteOutboundTxRequest, UpdateTransactionCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int) resContext.Items["Confirmations"]))
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.ToAddress));


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
