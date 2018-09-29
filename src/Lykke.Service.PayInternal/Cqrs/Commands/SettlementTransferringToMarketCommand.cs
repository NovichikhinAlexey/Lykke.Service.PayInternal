using ProtoBuf;
using System;

namespace Lykke.Service.PayInternal.Cqrs.Commands
{
    [ProtoContract]
    public class SettlementTransferringToMarketCommand
    {
        [ProtoMember(1, IsRequired = true)]
        public string PaymentRequestId { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public string MerchantId { get; set; }

        [ProtoMember(3, IsRequired = true)]
        public string TransactionHash { get; set; }

        [ProtoMember(4, IsRequired = true)]
        public string DestinationAddress { get; set; }

        [ProtoMember(5, IsRequired = true)]
        public Decimal TransactionAmount { get; set; }

        [ProtoMember(6, IsRequired = true)]
        public string TransactionAssetId { get; set; }
    }
}
