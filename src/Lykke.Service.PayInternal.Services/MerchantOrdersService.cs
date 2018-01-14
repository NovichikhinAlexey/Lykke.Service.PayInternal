using System;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.AzureRepositories.Order;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Services;

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

        public async Task<IOrder> CreateOrder(ICreateOrderRequest request)
        {
            var walletAddress =
                request.WalletAddress ?? await _merchantWalletsService.CreateAddress(request.MerchantId);

            var merchantMarkup = (await _merchantRepository.GetAsync(request.MerchantId)).GetMarkup();

            var rate = await _ratesCalculationService.GetRate(request.AssetPairId, request.MarkupPercent,
                request.MarkupPips, merchantMarkup);

            return await _ordersRepository.SaveAsync(new OrderEntity
            {
                AssetPairId = request.AssetPairId,
                DueDate = DateTime.UtcNow.Add(_orderExpiration),
                ExchangeAmount = request.InvoiceAmount / rate,
                ExchangeAssetId = request.ExchangeAssetId,
                InvoiceAmount = request.InvoiceAmount,
                InvoiceAssetId = request.InvoiceAssetId,
                InvoiceId = request.InvoiceId,
                MarkupPercent = request.MarkupPercent,
                MarkupPips = request.MarkupPips,
                MerchantId = request.MerchantId,
                ErrorUrl = request.ErrorUrl,
                SuccessUrl = request.SuccessUrl,
                ProgressUrl = request.ProgressUrl,
                WalletAddress = walletAddress
            });
        }
    }
}
