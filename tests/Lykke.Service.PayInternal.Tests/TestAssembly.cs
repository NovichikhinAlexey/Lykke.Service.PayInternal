using AutoMapper;
using Lykke.Service.PayInternal.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lykke.Service.PayInternal.Tests
{
    [TestClass]
    public class TestAssembly
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext testContext)
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
                cfg.AddProfile<AzureRepositories.AutoMapperProfile>();
                cfg.AddProfile<Services.AutoMapperProfile>();
            });

            Mapper.AssertConfigurationIsValid();
        }
    }
}
