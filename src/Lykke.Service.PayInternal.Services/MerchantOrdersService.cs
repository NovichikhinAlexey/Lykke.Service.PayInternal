using System;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.AzureRepositories.Order;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;

namespace Lykke.Service.PayInternal.Services
{
    public class MerchantOrdersService : IMerchantOrdersService
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly IMerchantWalletsService _merchantWalletsService;
        private readonly IMerchantRepository _merchantRepository;
        private readonly IRatesCalculationService _ratesCalculationService;
        private readonly TimeSpan _orderExpiration;

        public MerchantOrdersService(
            IOrdersRepository ordersRepository,
            IMerchantWalletsService merchantWalletsService,
            IMerchantRepository merchantRepository,
            IRatesCalculationService ratesCalculationService,
            TimeSpan orderExpiration)
        {
            _ordersRepository = ordersRepository ?? throw new ArgumentNullException(nameof(ordersRepository));
            _merchantWalletsService =
                merchantWalletsService ?? throw new ArgumentNullException(nameof(merchantWalletsService));
            _merchantRepository = merchantRepository ?? throw new ArgumentNullException(nameof(merchantRepository));
            _ratesCalculationService = ratesCalculationService ??
                                       throw new ArgumentNullException(nameof(ratesCalculationService));
            _orderExpiration = orderExpiration;
        }

        public async Task<IOrder> CreateOrder(ICreateOrder request)
        {
            var dueDate = DateTime.UtcNow.Add(_orderExpiration);

            var walletAddress = request.WalletAddress ??
                                await _merchantWalletsService.CreateAddress(request.ToWalletDomain(dueDate));

            var merchantMarkup = (await _merchantRepository.GetAsync(request.MerchantId)).GetMarkup();

            var rate = await _ratesCalculationService.GetRate(request.AssetPairId, request.MarkupPercent,
                request.MarkupPips, merchantMarkup);

            return await _ordersRepository.SaveAsync(new OrderEntity
            {
                AssetPairId = request.AssetPairId,
                DueDate = dueDate,
                ExchangeAmount = request.InvoiceAmount / rate,
                ExchangeAssetId = request.ExchangeAssetId,
                InvoiceAmount = request.InvoiceAmount,
                InvoiceAssetId = request.InvoiceAssetId,
                InvoiceId = request.InvoiceId,
                MarkupPercent = request.MarkupPercent,
                MarkupPips = request.MarkupPips,
                MerchantId = request.MerchantId,
                WalletAddress = walletAddress
            });
        }

        public async Task<IOrder> ReCreateOrder(IReCreateOrder request)
        {
            var ordersByAddress = await _ordersRepository.GetByWalletAsync(request.WalletAddress);

            var latestOrder = ordersByAddress.OrderByDescending(x => x.DueDate).FirstOrDefault();

            if (latestOrder == null)
                throw new Exception($"No orders created previously with wallet address {request.WalletAddress}");

            if (latestOrder.DueDate > DateTime.UtcNow) 
                return latestOrder;

            return await CreateOrder(new CreateOrder
            {
                WalletAddress = latestOrder.WalletAddress,
                MerchantId = latestOrder.MerchantId,
                InvoiceAmount = latestOrder.InvoiceAmount,
                InvoiceAssetId = latestOrder.InvoiceAssetId,
                ExchangeAssetId = latestOrder.ExchangeAssetId,
                AssetPairId = latestOrder.AssetPairId,
                MarkupPips = latestOrder.MarkupPips,
                MarkupPercent = latestOrder.MarkupPercent,
                InvoiceId = latestOrder.InvoiceId
            });
        }
    }
}
