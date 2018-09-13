using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Common.Log;
using Lykke.AzureStorage.Tables.Entity.Metamodel;
using Lykke.AzureStorage.Tables.Entity.Metamodel.Providers;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Common.Log;
using Lykke.Logs;

// ReSharper disable once RedundantUsingDirective
using Lykke.MonitoringServiceApiCaller;

using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings;
using Lykke.Service.PayInternal.Mapping;
using Lykke.Service.PayInternal.Modules;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.PayInternal
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; private set; }
        public IConfigurationRoot Configuration { get; }
        public ILog Log { get; private set; }
        private IHealthNotifier HealthNotifier { get; set; }

        // ReSharper disable once NotAccessedField.Local
        private string _monitoringServiceUrl;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.ContractResolver =
                            new Newtonsoft.Json.Serialization.DefaultContractResolver();
                    });

                services.AddSwaggerGen(opt => opt.DefaultLykkeConfiguration("v1", "PayInternal API"));

                EntityMetamodel.Configure(new AnnotationsBasedMetamodelProvider());

                var builder = new ContainerBuilder();

                var appSettings = Configuration.LoadSettings<AppSettings>(options =>
                {
                    options.SetConnString(x => x.SlackNotifications.AzureQueue.ConnectionString);
                    options.SetQueueName(x => x.SlackNotifications.AzureQueue.QueueName);
                    options.SenderName = "PayInternal API";
                });

                _monitoringServiceUrl = appSettings.CurrentValue.MonitoringServiceClient?.MonitoringServiceUrl;

                services.AddLykkeLogging(
                    appSettings.ConnectionString(x => x.PayInternalService.Db.LogsConnString),
                    "PayInternalLog",
                    appSettings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                    appSettings.CurrentValue.SlackNotifications.AzureQueue.QueueName);

                builder.RegisterModule(new AzureRepositories.AutofacModule(
                    appSettings.Nested(o => o.PayInternalService.Db.MerchantOrderConnString),
                    appSettings.Nested(o => o.PayInternalService.Db.MerchantConnString),
                    appSettings.Nested(o => o.PayInternalService.Db.PaymentRequestConnString),
                    appSettings.Nested(o => o.PayInternalService.Db.TransferConnString)));

                builder.RegisterModule(new Services.AutofacModule(
                    appSettings.CurrentValue.PayInternalService.ExpirationPeriods,
                    appSettings.CurrentValue.PayInternalService.TransactionConfirmationCount,
                    appSettings.CurrentValue.PayInternalService.Blockchain.WalletAllocationPolicy.Policies,
                    appSettings.CurrentValue.PayInternalService.AssetPairsLocalStorage.AssetPairs,
                    appSettings.CurrentValue.PayInternalService.CacheSettings,
                    appSettings.CurrentValue.PayInternalService.RetryPolicy,
                    appSettings.CurrentValue.PayInternalService.BilTransitionPeriodEnabled));

                builder.RegisterModule(new ServiceModule(appSettings));
                builder.RegisterModule(new CqrsModule(appSettings));

                builder.Populate(services);

                ApplicationContainer = builder.Build();

                Log = ApplicationContainer.Resolve<ILogFactory>().CreateLog(this);

                HealthNotifier = ApplicationContainer.Resolve<IHealthNotifier>();

                Mapper.Initialize(cfg =>
                {
                    cfg.ConstructServicesUsing(ApplicationContainer.Resolve);
                    cfg.AddProfiles(typeof(AutoMapperProfile));
                    cfg.AddProfiles(typeof(AzureRepositories.AutoMapperProfile));
                    cfg.AddProfiles(typeof(Services.AutoMapperProfile));
                });

                Mapper.AssertConfigurationIsValid();

                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                throw;
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseLykkeMiddleware(ex => new {Message = "Technical problem"});

                app.UseMvc();
                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
                });
                app.UseSwaggerUI(x =>
                {
                    x.RoutePrefix = "swagger/ui";
                    x.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                });
                app.UseStaticFiles();

                appLifetime.ApplicationStarted.Register(() =>
                {
                    try
                    {
                        StartApplication().GetAwaiter().GetResult();
                    }
                    catch (Exception)
                    {
                        appLifetime.StopApplication();
                    }
                });
                appLifetime.ApplicationStopping.Register(StopApplication);
                appLifetime.ApplicationStopped.Register(CleanUp);
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                // NOTE: Service not yet recieve and process requests here

                ApplicationContainer.Resolve<IStartupManager>().Start();

                HealthNotifier.Notify("Started");
#if !DEBUG
                if (!string.IsNullOrEmpty(_monitoringServiceUrl))
                    await Configuration.RegisterInMonitoringServiceAsync(_monitoringServiceUrl, HealthNotifier);
#endif
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                throw;
            }
        }

        private void StopApplication()
        {
            try
            {
                // NOTE: Service still can recieve and process requests here, so take care about it if you add logic here.

                ApplicationContainer.Resolve<IShutdownManager>().Stop();
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                throw;
            }
        }

        private void CleanUp()
        {
            try
            {
                HealthNotifier?.Notify("Terminating");

                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                (Log as IDisposable)?.Dispose();
                throw;
            }
        }
    }
}
