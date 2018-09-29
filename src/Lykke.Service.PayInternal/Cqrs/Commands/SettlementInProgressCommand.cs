using ProtoBuf;

namespace Lykke.Service.PayInternal.Cqrs.Commands
{
    [ProtoContract]
    public class SettlementInProgressCommand
    {
        [ProtoMember(1, IsRequired = true)]
        public string PaymentRequestId { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public string MerchantId { get; set; }
    }
}
