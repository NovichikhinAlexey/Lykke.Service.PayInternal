using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Common.Log;
using Lykke.Service.PayInternal.Core.Settings;
using Lykke.Service.PayInternal.Modules;
using Lykke.SettingsReader;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lykke.Service.PayInternal.Tests
{
    [TestClass]
    public class ControllerInitTests
    {
        public const string DefaultConfigurationKey = "SettingsUrl";

        [TestInitialize]
        public void Initialize()
        {
            string launchSettingsPath = Path.Combine("Properties", "launchSettings.json");

            if (File.Exists(launchSettingsPath))
            {
                using (var file = File.OpenText(launchSettingsPath))
                {
                    var reader = new JsonTextReader(file);

                    var jObject = JObject.Load(reader);

                    var variables = jObject
                        .GetValue("profiles")
                        .SelectMany(profiles => profiles.Children())
                        .SelectMany(profile => profile.Children<JProperty>())
                        .Where(prop => prop.Name == "environmentVariables")
                        .SelectMany(prop => prop.Value.Children<JProperty>())
                        .ToList();

                    foreach (var variable in variables)
                    {
                        Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                    }
                }
            }
        }

        [TestMethod]
        public void AllApiControllersInitializationTest()
        {
            var configBuilder = new ConfigurationBuilder();

            configBuilder.AddEnvironmentVariables();

            var configuration = configBuilder.Build();

            configuration[DefaultConfigurationKey] = configuration["SettingsUrlForApiTests"];

            var builder = new ContainerBuilder();

            var appSettings = configuration.LoadSettings<AppSettings>(DefaultConfigurationKey, true);

            builder.RegisterModule(new AzureRepositories.AutofacModule(
                appSettings.Nested(o => o.PayInternalService.Db.MerchantOrderConnString),
                appSettings.Nested(o => o.PayInternalService.Db.MerchantConnString),
                appSettings.Nested(o => o.PayInternalService.Db.PaymentRequestConnString),
                appSettings.Nested(o => o.PayInternalService.Db.TransferConnString),
                new Mock<ILog>().Object));

            builder.RegisterModule(new Services.AutofacModule(
                appSettings.CurrentValue.PayInternalService.ExpirationPeriods,
                appSettings.CurrentValue.PayInternalService.TransactionConfirmationCount,
                appSettings.CurrentValue.PayInternalService.Blockchain.WalletAllocationPolicy.Policies,
                appSettings.CurrentValue.PayInternalService.AssetPairsLocalStorage.AssetPairs,
                appSettings.CurrentValue.PayInternalService.CacheSettings));

            builder.RegisterModule(new ServiceModule(appSettings, new Mock<ILog>().Object));

            var controllerTypes = typeof(Startup).Assembly
                .GetExportedTypes()
                .Where(t => t.IsSubclassOf(typeof(Controller)))
                .ToList();

            foreach (var type in controllerTypes)
                builder.RegisterType(type);

            var ioc = builder.Build();

            var failedControllers = new List<string>();

            foreach (var type in controllerTypes)
            {
                try
                {
                    ioc.Resolve(type);
                }
                catch (Exception)
                {
                    failedControllers.Add(type.Name);
                }
            }

            if (failedControllers.Count > 0)
                throw new Exception($"These API controllers can't be instantiated: {string.Join(",", failedControllers)}");

            Assert.IsTrue(true);
        }
    }
}
