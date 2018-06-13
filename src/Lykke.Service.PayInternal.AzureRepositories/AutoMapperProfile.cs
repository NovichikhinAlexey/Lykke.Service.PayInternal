using System;
using AutoMapper;
using Lykke.Service.PayInternal.AzureRepositories.Asset;
using Lykke.Service.PayInternal.AzureRepositories.AssetPair;
using Lykke.Service.PayInternal.AzureRepositories.Markup;
using Lykke.Service.PayInternal.AzureRepositories.Merchant;
using Lykke.Service.PayInternal.AzureRepositories.PaymentRequest;
using Lykke.Service.PayInternal.AzureRepositories.Transaction;
using Lykke.Service.PayInternal.AzureRepositories.Transfer;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.AzureRepositories.Wallet;
using Lykke.Service.PayInternal.Core.Domain.Asset;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.AzureRepositories.MerchantGroup;
using Lykke.Service.PayInternal.AzureRepositories.SupervisorMembership;
using Lykke.Service.PayInternal.Core.Domain.Groups;
using Lykke.Service.PayInternal.Core.Domain.SupervisorMembership;
using Lykke.Service.PayInternal.AzureRepositories.File;
using Lykke.Service.PayInternal.AzureRepositories.MerchantWallet;
using Lykke.Service.PayInternal.Core.Domain.AssetPair;
using Lykke.Service.PayInternal.Core.Domain.File;
using Lykke.Service.PayInternal.Core.Domain.MerchantWallet;

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

            CreateMap<ISupervisorMembership, SupervisorMembershipEntity>(MemberList.Source)
                .ForMember(dest => dest.MerchantGroups,
                    opt => opt.MapFrom(src => string.Join(Constants.Separator, src.MerchantGroups)));

            CreateMap<SupervisorMembershipEntity, Core.Domain.SupervisorMembership.SupervisorMembership
            >(MemberList.Destination).ForMember(dest => dest.MerchantGroups,
                opt => opt.MapFrom(src => src.MerchantGroups.Split(Constants.Separator, StringSplitOptions.None)));

            CreateMap<IMerchantGroup, MerchantGroupEntity>(MemberList.Source)
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<MerchantGroupEntity, Core.Domain.Groups.MerchantGroup>(MemberList.Destination);

            CreateMap<FileInfo, FileInfoEntity>(MemberList.Source)
                .ForSourceMember(src => src.Id, opt => opt.Ignore());
                
            CreateMap<IAssetGeneralSettings, AssetGeneralSettingsEntity>(MemberList.Source);

            CreateMap<AssetGeneralSettingsEntity, AssetGeneralSettings>(MemberList.Destination);

            CreateMap<IMerchantWallet, MerchantWalletEntity>(MemberList.Source)
                .ForMember(dest => dest.Id, opt => opt.Ignore());
;
            CreateMap<MerchantWalletEntity, Core.Domain.MerchantWallet.MerchantWallet>(MemberList.Destination);

            CreateMap<IAssetPairRate, AssetPairRateEntity>(MemberList.Source);

            CreateMap<AssetPairRateEntity, AssetPairRate>(MemberList.Destination);
        }
    }
}
