using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using ProtoBuf;

namespace Lykke.Service.PayInternal.Cqrs.Commands
{
    [ProtoContract]
    public class SettlementErrorCommand
    {
        [ProtoMember(1, IsRequired = true)]
        public string PaymentRequestId { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public string MerchantId { get; set; }

        [ProtoMember(3, IsRequired = true)]
        public string ErrorDescription { get; set; }

        [ProtoMember(4, IsRequired = true)]
        public PaymentRequestProcessingError Error { get; set; }
    }
}
