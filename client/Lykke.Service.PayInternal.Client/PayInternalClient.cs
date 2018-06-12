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
using Lykke.Service.PayInternal.Client.Models.SupervisorMembership;
using Lykke.Service.PayInternal.Client.Models.Transactions;
using Lykke.Service.PayInternal.Client.Models.Wallets;
using Microsoft.Extensions.PlatformAbstractions;
using Refit;
using Lykke.Service.PayInternal.Client.Models.File;
using Lykke.Service.PayInternal.Client.Models.MerchantGroups;

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
        private readonly ISupervisorMembershipApi _supervisorMembershipApi;
        private readonly IFilesApi _filesApi;
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
            _supervisorMembershipApi = RestService.For<ISupervisorMembershipApi>(_httpClient);
            _filesApi = RestService.For<IFilesApi>(_httpClient);
            _runner = new ApiRunner();
        }

        public Task<IEnumerable<WalletStateResponse>> GetNotExpiredWalletsAsync()
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _payInternalApi.GetNotExpiredWalletsAsync());
        }

        public Task CreatePaymentTransactionAsync(CreateTransactionRequest request)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _payInternalApi.CreatePaymentTransactionAsync(request));
        }

        public Task UpdateTransactionAsync(UpdateTransactionRequest request)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _payInternalApi.UpdateTransactionAsync(request));
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

        public Task UpdateMerchantAsync(UpdateMerchantRequest request)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.UpdateAsync(request));
        }

        public Task SetMerchantPublicKeyAsync(string merchantId, byte[] content)
        {
            var streamPart = new StreamPart(new MemoryStream(content), "public.key");

            return _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.SetPublicKeyAsync(merchantId, streamPart));
        }

        public Task DeleteMerchantAsync(string merchantId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.DeleteAsync(merchantId));
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

        public Task<AvailableAssetsResponse> ResolveAvailableAssetsAsync(string merchantId, AssetAvailabilityType type)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.GetAvailableAssetsAsync(merchantId, type));
        }

        public Task<AvailableAssetsResponse> GetAvailableSettlementAssetsAsync(string merchantId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() =>
                _merchantsApi.GetAvailableSettlementAssetsAsync(merchantId));
        }

        public Task<AvailableAssetsResponse> GetAvailablePaymentAssetsAsync(string merchantId, string settlementAssetId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() =>
                _merchantsApi.GetAvailablePaymentAssetsAsync(merchantId, settlementAssetId));
        }

        public Task<IEnumerable<AssetGeneralSettingsResponse>> GetAssetGeneralSettingsAsync()
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _assetsApi.GetAssetGeneralSettingsAsync());
        }

        public Task<AssetMerchantSettingsResponse> GetAssetMerchantSettingsAsync(string merchantId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _assetsApi.GetAssetMerchantSettingsAsync(merchantId));
        }

        public Task SetAssetGeneralSettingsAsync(UpdateAssetGeneralSettingsRequest request)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _assetsApi.SetAssetGeneralSettingsAsync(request));
        }

        public Task SetAssetMerchantSettingsAsync(UpdateAssetMerchantSettingsRequest settingsRequest)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _assetsApi.SetAssetMerchantSettingsAsync(settingsRequest));
        }

        public Task CancelAsync(string merchantId, string paymentRequestId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _paymentRequestsApi.CancelAsync(merchantId, paymentRequestId));
        }

        public Task SetWalletExpiredAsync(BlockchainWalletExpiredRequest request)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _payInternalApi.SetWalletExpiredAsync(request));
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

        public Task SetDefaultMarkupAsync(string assetPairId, UpdateMarkupRequest request)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _markupsApi.SetDefaultAsync(assetPairId, request));
        }

        public Task<IReadOnlyList<MarkupResponse>> GetMarkupsForMerchantAsync(string merchantId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _markupsApi.GetForMerchantAsync(merchantId));
        }

        public Task<MarkupResponse> GetMarkupForMerchantAsync(string merchantId, string assetPairId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _markupsApi.GetForMerchantAsync(merchantId, assetPairId));
        }

        public Task SetMarkupForMerchantAsync(string merchantId, string assetPairId, UpdateMarkupRequest request)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _markupsApi.SetForMerchantAsync(merchantId, assetPairId, request));
        }

        public Task<SupervisorMembershipResponse> AddSupervisorMembershipAsync(AddSupervisorMembershipRequest request)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _supervisorMembershipApi.AddAsync(request));
        }

        public Task<SupervisorMembershipResponse> GetSupervisorMembershipAsync(string employeeId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _supervisorMembershipApi.GetAsync(employeeId));
        }

        public Task UpdateSupervisorMembershipAsync(UpdateSupervisorMembershipRequest request)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _supervisorMembershipApi.UpdateAsync(request));
        }

        public Task RemoveSupervisorMembershipAsync(string employeeId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _supervisorMembershipApi.RemoveAsync(employeeId));
        }

        public Task<MerchantsSupervisorMembershipResponse> AddSupervisorMembershipForMerchantsAsync(AddSupervisorMembershipMerchantsRequest request)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(
                () => _supervisorMembershipApi.AddForMerchantsAsync(request));
        }

        public Task<MerchantsSupervisorMembershipResponse> GetSupervisorMembershipWithMerchantsAsync(string employeeId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() =>
                _supervisorMembershipApi.GetWithMerchantsAsync(employeeId));
        }

        public Task SetTransactionExpiredAsync(TransactionExpiredRequest request)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _payInternalApi.SetTransactionExpiredAsync(request));
        }

        public async Task<IEnumerable<FileInfoModel>> GetFilesAsync(string merchantId)
        {
            return await _runner.RunWithDefaultErrorHandlingAsync(() => _filesApi.GetAllAsync(merchantId));
        }

        public async Task<byte[]> GetFileAsync(string merchantId, string fileId)
        {
            byte[] response = await _runner.RunWithDefaultErrorHandlingAsync(() => _filesApi.GetAsync(merchantId, fileId));

            return response;
        }

        public async Task UploadFileAsync(string merchantId, byte[] content, string fileName, string contentType)
        {
            var streamPart = new StreamPart(new MemoryStream(content), fileName, contentType);

            await _runner.RunWithDefaultErrorHandlingAsync(() => _filesApi.UploadAsync(merchantId, streamPart));
        }

        public async Task DeleteFileAsync(string merchantId, string fileId)
        {
            await _runner.RunWithDefaultErrorHandlingAsync(() => _filesApi.DeleteAsync(merchantId, fileId));
        }

        public Task<MerchantGroupResponse> AddMerchantGroupAsync(AddMerchantGroupRequest request)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.AddGroupAsync(request));
        }

        public Task<MerchantGroupResponse> GetMerchantGroupAsync(string id)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.GetGroupAsync(id));
        }

        public Task UpdateMerchantGroupAsync(UpdateMerchantGroupRequest request)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.UpdateGroupAsync(request));
        }

        public Task DeleteMerchantGroupAsync(string id)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.DeleteGroupAsync(id));
        }

        public Task<MerchantsByUsageResponse> GetMerchantsByUsageAsync(GetMerchantsByUsageRequest request)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.GetMerchantsByUsageAsync(request));
        }

        public Task<IEnumerable<MerchantGroupResponse>> GetMerchantGroupsByOwnerAsync(string ownerId)
        {
            return _runner.RunWithDefaultErrorHandlingAsync(() => _merchantsApi.GetGroupsByOwnerAsync(ownerId));
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
