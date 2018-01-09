using System;
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

        public async Task<string> CreateAddress(string merchantId)
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
                MerchantId = merchantId,
                Data = _cryptoService.Encrypt(JsonConvert.SerializeObject(addressToSave))
            });

            return addressToReturn;
        }
    }
}
