using AutoMapper;
using Lykke.Service.PayInternal.AzureRepositories.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;

namespace Lykke.Service.PayInternal.AzureRepositories
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<PaymentRequestEntity, Core.Domain.PaymentRequests.PaymentRequest>(MemberList.Destination);

            CreateMap<IPaymentRequest, PaymentRequestEntity>(MemberList.Source)
                .ForSourceMember(src => src.Id, opt => opt.Ignore());
        }
    }
}
