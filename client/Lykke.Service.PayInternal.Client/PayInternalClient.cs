using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Api;
using Lykke.Service.PayInternal.Client.Exceptions;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Lykke.Service.PayInternal.Client.Models.Markup;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInternal.Client.Models.Order;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInternal.Client.Models.Supervisor;
using Lykke.Service.PayInternal.Client.Models.Transactions;
using Lykke.Service.PayInternal.Client.Models.Wallets;
using Microsoft.Extensions.PlatformAbstractions;
using Refit;

namespace Lykke.Service.PayInternal.Client
{
    public class PayInternalClient : IPayInternalClient, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IPayInternalApi _payInternalApi;
        private readonly IMerchantsApi _merchantsApi;
        private readonly IOrdersApi _ordersApi;
        private readonly IPaymentRequestsApi _paymentRequestsApi;
        private readonly IAssetsApi _assetsApi;
        private readonly IMarkupsApi _markupsApi;
        private readonly ISupervisorApi _supervisorApi;
        private readonly ApiRunner _runner;

        public PayInternalClient(PayInternalServiceClientSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrEmpty(settings.ServiceUrl))
                throw new Exception("Service URL required");

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(settings.ServiceUrl),
                DefaultRequestHeaders =
                {
                    {
                        "User-Agent",
                        $"{PlatformServices.Default.Application.ApplicationName}/{PlatformServices.Default.Application.ApplicationVersion}"
                    }
                }
            };

            _payInternalApi = RestService.For<IPayInternalApi>(_httpClient);
            _merchantsApi = RestService.For<IMerchantsApi>(_httpClient);
            _ordersApi = RestService.For<IOrdersApi>(_httpClient);
            _paymentRequestsApi = RestService.For<IPaymentRequestsApi>(_httpClient);
            _assetsApi = RestService.For<IAssetsApi>(_httpClient);
            _markupsApi = RestService.For<IMarkupsApi>(_httpClient);
            _supervisorApi = RestService.For<ISupervisorApi>(_httpClient);
            _runner = new ApiRunner();
        }

        public Task<IEnumerable<WalletStateResponse>> GetNotExpiredWalletsAsync()
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _payInternalApi.GetNotExpiredWalletsAsync());
        }

        public async Task CreatePaymentTransactionAsync(CreateTransactionRequest request)
        {
            await _runner.RunWithDefaultErrorHandlingAsync(() => _payInternalApi.CreatePaymentTransactionAsync(request));
        }

        public async Task UpdateTransactionAsync(UpdateTransactionRequest request)
        {
            await _runner.RunWithDefaultErrorHandlingAsync(() => _payInternalApi.UpdateTransactionAsync(request));
        }

        public Task<IReadOnlyList<MerchantModel>> GetMerchantsAsync()
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.GetAllAsync());
        }
        
        public Task<MerchantModel> GetMerchantByIdAsync(string merchantId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.GetByIdAsync(merchantId));
        }

        public Task<MerchantModel> CreateMerchantAsync(CreateMerchantRequest request)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.CreateAsync(request));
        }

        public async Task UpdateMerchantAsync(UpdateMerchantRequest request)
        {
            await _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.UpdateAsync(request));
        }

        public async Task SetMerchantPublicKeyAsync(string merchantId, byte[] content)
        {
            var streamPart = new StreamPart(new MemoryStream(content), "public.key");

            await _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.SetPublicKeyAsync(merchantId, streamPart));
        }

        public async Task DeleteMerchantAsync(string merchantId)
        {
            await _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.DeleteAsync(merchantId));
        }
        
        public Task<OrderModel> GetOrderAsync(string merchantId, string paymentRequestId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _ordersApi.GetByIdAsync(merchantId, paymentRequestId));
        }

        public Task<OrderModel> ChechoutOrderAsync(ChechoutRequestModel model)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _ordersApi.ChechoutAsync(model));
        }

        public Task<IReadOnlyList<PaymentRequestModel>> GetPaymentRequestsAsync(string merchantId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _paymentRequestsApi.GetAllAsync(merchantId));
        }

        public Task<PaymentRequestModel> GetPaymentRequestAsync(string merchantId, string paymentRequestId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _paymentRequestsApi.GetAsync(merchantId, paymentRequestId));
        }

        public Task<PaymentRequestDetailsModel> GetPaymentRequestDetailsAsync(string merchantId, string paymentRequestId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _paymentRequestsApi.GetDetailsAsync(merchantId, paymentRequestId));
        }

        public Task<PaymentRequestModel> GetPaymentRequestByAddressAsync(string walletAddress)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _paymentRequestsApi.GetByAddressAsync(walletAddress));
        }

        public Task<PaymentRequestModel> CreatePaymentRequestAsync(CreatePaymentRequestModel model)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _paymentRequestsApi.CreateAsync(model));
        }
        
        public Task<BtcTransferResponse> BtcFreeTransferAsync(BtcFreeTransferRequest request)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _paymentRequestsApi.BtcFreeTransferAsync(request));
        }

        public Task<IEnumerable<TransactionStateResponse>> GetAllMonitoredTransactionsAsync()
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _payInternalApi.GetAllMonitoredTransactionsAsync());
        }

        public Task<RefundResponse> RefundAsync(RefundRequestModel request)
        {
            return _runner.RunAsync(() => _paymentRequestsApi.RefundAsync(request), ExceptionFactories.CreateRefundException);
        }

        public async Task<AvailableAssetsResponse> ResolveAvailableAssetsAsync(string merchantId, AssetAvailabilityType type)
        {
            return await _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.GetAvailableAssetsAsync(merchantId, type));
        }

        public async Task<AvailableAssetsResponse> GetAvailableSettlementAssetsAsync(string merchantId)
        {
            return await _runner.RunWithDefaultErrorHandlingAsync(() =>
                _merchantsApi.GetAvailableSettlementAssetsAsync(merchantId));
        }

        public async Task<AvailableAssetsResponse> GetAvailablePaymentAssetsAsync(string merchantId, string settlementAssetId)
        {
            return await _runner.RunWithDefaultErrorHandlingAsync(() =>
                _merchantsApi.GetAvailablePaymentAssetsAsync(merchantId, settlementAssetId));
        }

        public async Task<IEnumerable<AssetGeneralSettingsResponse>> GetAssetGeneralSettingsAsync()
        {
            return await _runner.RunWithDefaultErrorHandlingAsync(() => _assetsApi.GetAssetGeneralSettingsAsync());
        }

        public Task<AssetMerchantSettingsResponse> GetAssetMerchantSettingsAsync(string merchantId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _assetsApi.GetAssetMerchantSettingsAsync(merchantId));
        }

        public async Task SetAssetGeneralSettingsAsync(UpdateAssetGeneralSettingsRequest request)
        {
            await _runner.RunWithDefaultErrorHandlingAsync(() => _assetsApi.SetAssetGeneralSettingsAsync(request));
        }

        public async Task SetAssetMerchantSettingsAsync(UpdateAssetMerchantSettingsRequest settingsRequest)
        {
            await _runner.RunWithDefaultErrorHandlingAsync(() => _assetsApi.SetAssetMerchantSettingsAsync(settingsRequest));
        }

        public async Task CancelAsync(string merchantId, string paymentRequestId)
        {
            await _runner.RunWithDefaultErrorHandlingAsync(() => _paymentRequestsApi.CancelAsync(merchantId, paymentRequestId));
        }

        public async Task SetWalletExpiredAsync(BlockchainWalletExpiredRequest request)
        {
            await _runner.RunWithDefaultErrorHandlingAsync(() => _payInternalApi.SetWalletExpiredAsync(request));
        }

        public Task<MarkupResponse> ResolveMarkupByMerchantAsync(string merchantId, string assetPairId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.ResolveMarkupAsync(merchantId, assetPairId));
        }

        public Task<IReadOnlyList<MarkupResponse>> GetDefaultMarkupsAsync()
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _markupsApi.GetDefaultsAsync());
        }

        public Task<MarkupResponse> GetDefaultMarkupAsync(string assetPairId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _markupsApi.GetDefaultAsync(assetPairId));
        }

        public async Task SetDefaultMarkupAsync(string assetPairId, UpdateMarkupRequest request)
        {
            await _runner.RunWithDefaultErrorHandlingAsync(() => _markupsApi.SetDefaultAsync(assetPairId, request));
        }

        public Task<IReadOnlyList<MarkupResponse>> GetMarkupsForMerchantAsync(string merchantId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _markupsApi.GetForMerchantAsync(merchantId));
        }

        public Task<MarkupResponse> GetMarkupForMerchantAsync(string merchantId, string assetPairId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _markupsApi.GetForMerchantAsync(merchantId, assetPairId));
        }

        public async Task SetMarkupForMerchantAsync(string merchantId, string assetPairId, UpdateMarkupRequest request)
        {
            await _runner.RunWithDefaultErrorHandlingAsync(() => _markupsApi.SetForMerchantAsync(merchantId, assetPairId, request));
        }

        public async Task SetTransactionExpiredAsync(TransactionExpiredRequest request)
        {
            await _runner.RunWithDefaultErrorHandlingAsync(() => _payInternalApi.SetTransactionExpiredAsync(request));
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        public async Task<SupervisingMerchantsResponse> GetSupervisingMerchantsAsync(string employeeId)
        {
            return await _runner.RunWithDefaultErrorHandlingAsync(() => _supervisorApi.GetSupervisingMerchantsAsync(employeeId));
        }

        public async Task DeleteSupervisingAsync(string employeeId)
        {
            await _runner.RunWithDefaultErrorHandlingAsync(() => _supervisorApi.DeleteSupervisingMerchantsAsync(employeeId));
        }

        public async Task SetSupervisingMerchantsAsync(CreateSupervisingEmployeeRequest request)
        {
            await _runner.RunWithDefaultErrorHandlingAsync(() => _supervisorApi.SetSupervisingMerchantsAsync(request));
        }
    }
}
