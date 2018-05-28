using AutoMapper;
using Lykke.Service.PayInternal.AzureRepositories.Markup;
using Lykke.Service.PayInternal.AzureRepositories.Merchant;
using Lykke.Service.PayInternal.AzureRepositories.PaymentRequest;
using Lykke.Service.PayInternal.AzureRepositories.Transaction;
using Lykke.Service.PayInternal.AzureRepositories.Transfer;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.AzureRepositories.Wallet;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.AzureRepositories.File;
using Lykke.Service.PayInternal.Core.Domain.File;

namespace Lykke.Service.PayInternal.AzureRepositories
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<PaymentRequestEntity, Core.Domain.PaymentRequests.PaymentRequest>(MemberList.Destination)
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.CreatedOn));

            CreateMap<IPaymentRequest, PaymentRequestEntity>(MemberList.Source)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Timestamp, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => src.Timestamp));

            CreateMap<PaymentRequestTransactionEntity, PaymentRequestTransaction>(MemberList.Destination)
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => src.Timestamp));

            CreateMap<IPaymentRequestTransaction, PaymentRequestTransactionEntity>(MemberList.Source)
                .ForSourceMember(src => src.Id, opt => opt.Ignore())
                .ForSourceMember(src => src.CreatedOn, opt => opt.Ignore());

            CreateMap<TransferEntity, Core.Domain.Transfer.Transfer>(MemberList.Destination);

            CreateMap<IMerchant, MerchantEntity>(MemberList.Source)
                .ForSourceMember(src => src.Id, opt => opt.Ignore());

            CreateMap<VirtualWalletEntity, VirtualWallet>(MemberList.Destination);

            CreateMap<BcnWalletUsageEntity, BcnWalletUsage>(MemberList.Destination);

            CreateMap<IMarkup, MarkupEntity>(MemberList.Source);

            CreateMap<MarkupEntity, Core.Domain.Markup.Markup>(MemberList.Destination);

            CreateMap<FileInfoEntity, FileInfo>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.RowKey));

            CreateMap<FileInfo, FileInfoEntity>(MemberList.Source)
                .ForSourceMember(src => src.Id, opt => opt.Ignore());
        }
    }
}
