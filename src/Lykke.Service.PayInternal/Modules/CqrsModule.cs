using Autofac;
using Lykke.Common.Chaos;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Messaging;
using Lykke.Messaging.RabbitMq;
using Lykke.Service.PayInternal.Core.Settings;
using Lykke.Service.PaySettlement.Contracts.Events;
using Lykke.SettingsReader;
using System.Collections.Generic;
using Lykke.Service.PayInternal.Cqrs.CommandHandlers;
using Lykke.Service.PayInternal.Cqrs.Commands;
using Lykke.Service.PayInternal.Cqrs.Sagas;
using Lykke.Service.PaySettlement.Contracts.Commands;

namespace Lykke.Service.PayInternal.Modules
{
    public class CqrsModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;
        public const string InternalBoundedContext = "lykkepay-internal";
        private const string CommandsRoute = "commands";

        public CqrsModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterChaosKitty(builder);

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

            RegisterComponents(builder);

            const string settlementTransport = "Settlement";
            var mainSettings = new RabbitMQ.Client.ConnectionFactory
            {
                Uri = _appSettings.CurrentValue.PaySettlementCqrs.ConnectionString
            };

            builder.Register(ctx =>
            {
                var logFactory = ctx.Resolve<ILogFactory>();

                var settlementBroker = mainSettings.Endpoint.ToString();

                var settlementMessagingEngine = new MessagingEngine(logFactory,
                    new TransportResolver(new Dictionary<string, TransportInfo>
                    {
                        {
                            settlementTransport,
                            new TransportInfo(settlementBroker, mainSettings.UserName, mainSettings.Password, "None",
                                _appSettings.CurrentValue.PaySettlementCqrs.Messaging)
                        }
                    }),
                    new RabbitMqTransportFactory(logFactory));

                return new CqrsEngine(logFactory,
                    ctx.Resolve<IDependencyResolver>(),
                    settlementMessagingEngine,
                    new DefaultEndpointProvider(),
                    true,
                    Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver(
                        settlementTransport,
                        _appSettings.CurrentValue.PaySettlementCqrs.SerializationFormat,
                        environment: _appSettings.CurrentValue.PaySettlementCqrs.Environment)),

                    Register.BoundedContext(InternalBoundedContext)
                        .ListeningCommands(typeof(SettlementInProgressCommand),
                            typeof(SettlementTransferringToMarketCommand),
                            typeof(SettledCommand), typeof(SettlementErrorCommand))
                        .On(CommandsRoute)
                        .WithCommandsHandler<SettlementCommandHandler>(),

                    Register.Saga<InternalSaga>("lykkepay-internal-saga")
                        .PublishingCommands(typeof(SettlementInProgressCommand),
                            typeof(SettlementTransferringToMarketCommand),
                            typeof(SettledCommand), typeof(SettlementErrorCommand))
                        .To(InternalBoundedContext).With(CommandsRoute)
                        .ListeningEvents(typeof(SettlementTransferToMarketQueuedEvent),
                            typeof(SettlementTransferringToMarketEvent),
                            typeof(SettlementTransferredToMerchantEvent), typeof(SettlementErrorEvent))
                        .From(_appSettings.CurrentValue.PaySettlementCqrs.SettlementBoundedContext)
                        .On(_appSettings.CurrentValue.PaySettlementCqrs.EventsRoute)
                );
            })
            .As<ICqrsEngine>().SingleInstance().AutoActivate();
        }

        private void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<InternalSaga>();
            builder.RegisterType<SettlementCommandHandler>()
                .WithParameter("transactionConfirmationCount",
                    _appSettings.CurrentValue.PayInternalService.TransactionConfirmationCount);
        }

        private void RegisterChaosKitty(ContainerBuilder builder)
        {
            if (_appSettings.CurrentValue.ChaosKitty != null)
            {
                builder.RegisterType<ChaosKitty>()
                    .WithParameter(TypedParameter.From(_appSettings.CurrentValue.ChaosKitty.StateOfChaos))
                    .As<IChaosKitty>()
                    .SingleInstance();
            }
            else
            {
                builder.RegisterType<SilentChaosKitty>()
                    .As<IChaosKitty>()
                    .SingleInstance();
            }
        }
    }
}
