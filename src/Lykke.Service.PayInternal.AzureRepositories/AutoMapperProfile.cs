using AutoMapper;
using Lykke.Service.PayInternal.AzureRepositories.PaymentRequest;
using Lykke.Service.PayInternal.AzureRepositories.Transaction;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.AzureRepositories
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<PaymentRequestEntity, Core.Domain.PaymentRequests.PaymentRequest>(MemberList.Destination);

            CreateMap<IPaymentRequest, PaymentRequestEntity>(MemberList.Source)
                .ForSourceMember(src => src.Id, opt => opt.Ignore());

            CreateMap<PaymentRequestTransactionEntity, PaymentRequestTransaction>(MemberList.Destination);

            CreateMap<IPaymentRequestTransaction, PaymentRequestTransactionEntity>(MemberList.Source)
                .ForSourceMember(src => src.Id, opt => opt.Ignore());
        }
    }
}
