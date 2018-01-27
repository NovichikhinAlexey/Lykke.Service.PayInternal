using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.PayInternal.Core.Domain.Merchant;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;

namespace Lykke.Service.PayInternal.Services
{
    public class MerchantService : IMerchantService
    {
        private readonly IMerchantRepository _merchantRepository;
        private readonly ILog _log;

        public MerchantService(
            IMerchantRepository merchantRepository,
            ILog log)
        {
            _merchantRepository = merchantRepository;
            _log = log;
        }
        
        public async Task<IReadOnlyList<IMerchant>> GetAsync()
        {
            return await _merchantRepository.GetAsync();
        }

        public async Task<IMerchant> GetAsync(string merchantId)
        {
            return await _merchantRepository.GetAsync(merchantId);
        }

        public async Task<IMerchant> CreateAsync(IMerchant merchant)
        {
            IMerchant createdMerchant = await _merchantRepository.InsertAsync(merchant);

            await _log.WriteInfoAsync(nameof(MerchantService), nameof(CreateAsync),
                merchant.ToContext(),
                "Merchant created");

            return createdMerchant;
        }

        public async Task UpdateAsync(IMerchant merchant)
        {
            IMerchant existingMerchant = await _merchantRepository.GetAsync(merchant.Id);
            
            if(existingMerchant == null)
                throw new MerchantNotFoundException(merchant.Id);

            existingMerchant.Name = merchant.Name;
            existingMerchant.ApiKey = merchant.ApiKey;
            existingMerchant.DeltaSpread = merchant.DeltaSpread;
            existingMerchant.TimeCacheRates = merchant.TimeCacheRates;
            existingMerchant.LpMarkupPercent = merchant.LpMarkupPercent;
            existingMerchant.LpMarkupPips = merchant.LpMarkupPips;
            existingMerchant.LwId = merchant.LwId;
            
            await _merchantRepository.ReplaceAsync(existingMerchant);
            
            await _log.WriteInfoAsync(nameof(MerchantService), nameof(UpdateAsync),
                merchant.ToContext(),
                "Merchant updated");
        }

        public async Task SetPublicKeyAsync(string merchantId, string publicKey)
        {
            IMerchant merchant = await _merchantRepository.GetAsync(merchantId);
            
            if(merchant == null)
                throw new MerchantNotFoundException(merchantId);
            
            merchant.PublicKey = publicKey;
            
            await _merchantRepository.ReplaceAsync(merchant);
            
            await _log.WriteInfoAsync(nameof(MerchantService), nameof(SetPublicKeyAsync),
                merchant.ToContext(),
                "Merchant public key updated");
        }

        public async Task DeleteAsync(string merchantId)
        {
            await _merchantRepository.DeleteAsync(merchantId);
            
            await _log.WriteInfoAsync(nameof(MerchantService), nameof(DeleteAsync),
                new{MerchantId = merchantId}.ToJson(),
                "Merchant deleted");
        }
    }
}
