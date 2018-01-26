using Autofac;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Common.Log;
using Lykke.Service.PayInternal.AzureRepositories.Merchant;
using Lykke.Service.PayInternal.AzureRepositories.Order;
using Lykke.Service.PayInternal.AzureRepositories.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;
using Lykke.SettingsReader;

namespace Lykke.Service.PayInternal.AzureRepositories
{
    public class AutofacModule : Module
    {
        private readonly IReloadingManager<string> _ordersConnectionString;
        private readonly IReloadingManager<string> _merchantsConnectionString;
        private readonly IReloadingManager<string> _paymentRequestsConnectionString;
        private readonly ILog _log;

        public AutofacModule(
            IReloadingManager<string> ordersConnectionString,
            IReloadingManager<string> merchantsConnectionString,
            IReloadingManager<string> paymentRequestsConnectionString,
            ILog log)
        {
            _ordersConnectionString = ordersConnectionString;
            _merchantsConnectionString = merchantsConnectionString;
            _paymentRequestsConnectionString = paymentRequestsConnectionString;
            _log = log;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            const string merchantsTableName = "Merchants";
            const string paymentRequestsTableName = "PaymentRequests";
            const string ordersTableName = "Orders";

            builder.RegisterInstance<IMerchantRepository>(new MerchantRepository(
                AzureTableStorage<MerchantEntity>.Create(_merchantsConnectionString,
                    merchantsTableName, _log)));

            builder.RegisterInstance<IPaymentRequestRepository>(new PaymentRequestRepository(
                AzureTableStorage<PaymentRequestEntity>.Create(_paymentRequestsConnectionString,
                    paymentRequestsTableName, _log),
                AzureTableStorage<AzureIndex>.Create(_paymentRequestsConnectionString,
                    paymentRequestsTableName, _log)));
            
            builder.RegisterInstance<IOrderRepository>(new OrdersRepository(
                AzureTableStorage<OrderEntity>.Create(_ordersConnectionString,
                    ordersTableName, _log)));
        }
    }
}
