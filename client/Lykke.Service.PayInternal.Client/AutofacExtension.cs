using System;
using Autofac;
using Common.Log;

namespace Lykke.Service.PayInternal.Client
{
    public static class AutofacExtension
    {
        public static void RegisterPayInternalClient(this ContainerBuilder builder, string serviceUrl, ILog log)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (serviceUrl == null) throw new ArgumentNullException(nameof(serviceUrl));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (string.IsNullOrWhiteSpace(serviceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));

            builder.RegisterType<PayInternalClient>()
                .WithParameter("serviceUrl", serviceUrl)
                .As<IPayInternalClient>()
                .SingleInstance();
        }

        public static void RegisterPayInternalClient(this ContainerBuilder builder, PayInternalServiceClientSettings settings, ILog log)
        {
            builder.RegisterPayInternalClient(settings?.ServiceUrl, log);
        }
    }
}
