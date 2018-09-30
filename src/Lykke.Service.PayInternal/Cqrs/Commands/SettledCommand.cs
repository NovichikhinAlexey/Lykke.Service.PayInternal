using ProtoBuf;
using System;

namespace Lykke.Service.PayInternal.Cqrs.Commands
{
    [ProtoContract]
    public class SettledCommand
    {
        [ProtoMember(1, IsRequired = true)]
        public string PaymentRequestId { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public string MerchantId { get; set; }

        [ProtoMember(3, IsRequired = true)]
        public Decimal TransferredAmount { get; set; }

        [ProtoMember(4, IsRequired = true)]
        public string TransferredAssetId { get; set; }
    }
}
