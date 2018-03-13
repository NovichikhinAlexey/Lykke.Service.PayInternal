using AutoMapper;
using Lykke.Service.PayInternal.Contract.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Models;
using Lykke.Service.PayInternal.Models.Orders;
using Lykke.Service.PayInternal.Models.PaymentRequests;
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

            PaymentRequestApiModels();
            PaymentRequestMessages();
        }

        private void PaymentRequestApiModels()
        {
            CreateMap<IPaymentRequest, PaymentRequestModel>(MemberList.Source);
            
            CreateMap<CreatePaymentRequestModel, PaymentRequest>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.PaidAmount, opt => opt.Ignore())
                .ForMember(dest => dest.PaidDate, opt => opt.Ignore())
                .ForMember(dest => dest.Error, opt => opt.Ignore());

            CreateMap<IPaymentRequest, PaymentRequestDetailsModel>(MemberList.Source)
                .ForSourceMember(dest => dest.OrderId, opt => opt.Ignore());

            CreateMap<IOrder, PaymentRequestOrderModel>(MemberList.Source)
                .ForSourceMember(src => src.MerchantId, opt => opt.Ignore())
                .ForSourceMember(src => src.PaymentRequestId, opt => opt.Ignore())
                .ForSourceMember(src => src.AssetPairId, opt => opt.Ignore())
                .ForSourceMember(src => src.SettlementAmount, opt => opt.Ignore());

            CreateMap<IBlockchainTransaction, PaymentRequestTransactionModel>(MemberList.Source)
                .ForSourceMember(src => src.Id, opt => opt.Ignore())
                .ForSourceMember(src => src.PaymentRequestId, opt => opt.Ignore())
                .ForSourceMember(src => src.WalletAddress, opt => opt.Ignore())
                .ForSourceMember(src => src.Blockchain, opt => opt.Ignore())
                .ForSourceMember(src => src.TransactionType, opt => opt.Ignore())
                .ForSourceMember(src => src.DueDate, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TransactionId))
                .ForMember(dest => dest.Url, opt => opt.ResolveUsing<TransactionUrlValueResolver>())
                .ForMember(dest => dest.RefundUrl, opt => opt.Ignore());
        }

        private void PaymentRequestMessages()
        {
            CreateMap<IPaymentRequest, PaymentRequestDetailsMessage>(MemberList.Source)
                .ForSourceMember(dest => dest.OrderId, opt => opt.Ignore());

            CreateMap<IOrder, PaymentRequestOrder>(MemberList.Source)
                .ForSourceMember(src => src.MerchantId, opt => opt.Ignore())
                .ForSourceMember(src => src.PaymentRequestId, opt => opt.Ignore())
                .ForSourceMember(src => src.AssetPairId, opt => opt.Ignore())
                .ForSourceMember(src => src.SettlementAmount, opt => opt.Ignore());

            CreateMap<IBlockchainTransaction, PaymentRequestTransaction>(MemberList.Source)
                .ForSourceMember(src => src.Id, opt => opt.Ignore())
                .ForSourceMember(src => src.PaymentRequestId, opt => opt.Ignore())
                .ForSourceMember(src => src.WalletAddress, opt => opt.Ignore())
                .ForSourceMember(src => src.AssetId, opt => opt.Ignore())
                .ForSourceMember(src => src.Blockchain, opt => opt.Ignore())
                .ForSourceMember(src => src.TransactionType, opt => opt.Ignore())
                .ForSourceMember(src => src.DueDate, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TransactionId));            
        }
    }
}
