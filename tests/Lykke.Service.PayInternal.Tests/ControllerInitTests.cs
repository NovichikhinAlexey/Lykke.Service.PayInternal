using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Lykke.Service.PayInternal.Tests
{
    [TestClass]
    public class ControllerInitTests
    {
        [TestMethod]
        public void AllApiControllersINitializationTest()
        {
            //var configBuilder = new ConfigurationBuilder();

            //configBuilder.AddEnvironmentVariables();

            //var configuration = configBuilder.Build();

            //configuration["SettingsUrl"] = configuration["SettingsUrlForApiTests"];

            //var binder = new AzureBinder();
            //var builder = binder.Bind(configuration, null, true);
            //builder.RegisterInstance(new Mock<IHostingEnvironment>().Object).As<IHostingEnvironment>();
            //builder.RegisterInstance(new Mock<IHttpContextAccessor>().Object).As<IHttpContextAccessor>();
            //var controllerTypes = typeof(Startup).Assembly
            //    .GetExportedTypes()
            //    .Where(t => t.IsSubclassOf(typeof(Controller)))
            //    .ToList();

            //foreach (var type in controllerTypes)
            //    builder.RegisterType(type);

            //var ioc = builder.Build();

            //var failedControllers = new List<string>();
            //foreach (var type in controllerTypes)
            //{
            //    try
            //    {
            //        ioc.Resolve(type);
            //    }
            //    catch (Exception)
            //    {
            //        failedControllers.Add(type.Name);
            //    }
            //}
            //if (failedControllers.Count > 0)
            //    throw new Exception($"These WalletApi controllers can't be instantiated: {string.Join(",", failedControllers)}");
            //Assert.IsTrue(true);
        }
    }
}
