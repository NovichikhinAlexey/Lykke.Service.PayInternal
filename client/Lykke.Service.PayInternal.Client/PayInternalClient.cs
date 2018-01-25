using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Api;
using Lykke.Service.PayInternal.Client.Models;
using Microsoft.Extensions.PlatformAbstractions;
using Refit;

namespace Lykke.Service.PayInternal.Client
{
    public class PayInternalClient : IPayInternalClient, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IPayInternalApi _payInternalApi;
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
            _runner = new ApiRunner();
        }

        public async Task<WalletAddressResponse> CreateAddressAsync(CreateWalletRequest request)
        {
            return await _runner.RunAsync(() => _payInternalApi.CreateAddressAsync(request));
        }

        public async Task<IEnumerable<WalletStateResponse>> GetNotExpiredWalletsAsync()
        {
            return await _runner.RunAsync(() => _payInternalApi.GetNotExpiredWalletsAsync());
        }

        public async Task CreateMerchantAsync(CreateMerchantRequest request)
        {
            await _runner.RunAsync(() => _payInternalApi.CreateMerchantAsync(request));
        }

        public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request)
        {
            return await _runner.RunAsync(() => _payInternalApi.CreateOrderAsync(request));
        }

        public async Task<CreateOrderResponse> ReCreateOrderAsync(ReCreateOrderRequest request)
        {
            return await _runner.RunAsync(() => _payInternalApi.ReCreateOrderAsync(request));
        }

        public async Task UpdatePublicKeyAsync(byte[] content, string id, string fileName, string contentType)
        {
            var streamPart = new StreamPart(new MemoryStream(content), fileName, contentType);

            await _runner.RunAsync(() => _payInternalApi.UpdatePublicKeyAsync(streamPart, id));
        }

        public async Task CreateTransaction(CreateTransactionRequest request)
        {
            await _runner.RunAsync(() => _payInternalApi.CreateTransaction(request));
        }

        public async Task UpdateTransaction(UpdateTransactionRequest request)
        {
            await _runner.RunAsync(() => _payInternalApi.UpdateTransaction(request));
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
