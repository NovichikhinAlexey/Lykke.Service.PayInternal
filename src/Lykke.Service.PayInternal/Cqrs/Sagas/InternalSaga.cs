using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Service.PayInternal.Cqrs.Commands;
using Lykke.Service.PayInternal.Modules;
using Lykke.Service.PaySettlement.Contracts.Events;

namespace Lykke.Service.PayInternal.Cqrs.Sagas
{
    [UsedImplicitly]
    public class InternalSaga
    {
        [UsedImplicitly]
        public void Handle(SettlementTransferToMarketQueuedEvent e, ICommandSender commandSender)
        {
            commandSender.SendCommand(Mapper.Map<SettlementInProgressCommand>(e),
                CqrsModule.InternalBoundedContext);
        }

        [UsedImplicitly]
        public void Handle(SettlementTransferredToMerchantEvent e, ICommandSender commandSender)
        {
            commandSender.SendCommand(Mapper.Map<SettledCommand>(e),
                CqrsModule.InternalBoundedContext);
        }

        [UsedImplicitly]
        public void Handle(SettlementErrorEvent e, ICommandSender commandSender)
        {
            commandSender.SendCommand(Mapper.Map<SettlementErrorCommand>(e),
                CqrsModule.InternalBoundedContext);
        }
    }
}
