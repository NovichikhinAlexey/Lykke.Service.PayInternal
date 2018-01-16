using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Lykke.Bitcoin.Api.Client.BitcoinApi;
using Lykke.Service.PayInternal.AzureRepositories.Wallet;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Core.Services;
using Newtonsoft.Json;

namespace Lykke.Service.PayInternal.Services
{
    public class MerchantWalletsService : IMerchantWalletsService
    {
        private readonly IBitcoinApiClient _bitcoinServiceClient;
        private readonly IWalletRepository _walletRepository;
        private readonly ICryptoService _cryptoService;

        public MerchantWalletsService(
            IBitcoinApiClient bitcoinServiceClient,
            IWalletRepository walletRepository,
            ICryptoService cryptoService)
        {
            _bitcoinServiceClient =
                bitcoinServiceClient ?? throw new ArgumentNullException(nameof(bitcoinServiceClient));
            _walletRepository = walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
        }

        public async Task<string> CreateAddress(ICreateWalletRequest request)
        {
            var response = await _bitcoinServiceClient.GenerateLykkePayWallet();

            if (response.HasError)
            {
                throw new Exception(response.Error?.ToJson());
            }

            var addressToReturn = response.Address;

            var addressToSave = new WalletAddress
            {
                Address = response.Address,
                PublicKey = response.PubKey
            };

            await _walletRepository.SaveAsync(new WalletEntity
            {
                Address = response.Address,
                MerchantId = request.MerchantId,
                Data = _cryptoService.Encrypt(JsonConvert.SerializeObject(addressToSave)),
                Amount = default(double),
                DueDate = request.DueDate
            });

            return addressToReturn;
        }

        public async Task<IWallet> GetAsync(string merchantId, string address)
        {
            return await _walletRepository.GetAsync(merchantId, address);
        }

        public async Task<IEnumerable<IWallet>> GetAsync(string merchantId)
        {
            return await _walletRepository.GetByMerchantAsync(merchantId);
        }

        public async Task<IEnumerable<IWallet>> GetNonEmptyAsync(string merchantId)
        {
            return await _walletRepository.GetByMerchantAsync(merchantId, true);
        }

        public async Task<IEnumerable<IWallet>> GetNotExpired()
        {
            return await _walletRepository.GetNotExpired();
        }
    }
}
