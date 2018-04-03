using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
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

        public async Task<IMerchant> GetAsync(string merchantName)
        {
            return await _merchantRepository.GetAsync(merchantName);
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
            IMerchant existingMerchant = await _merchantRepository.GetAsync(merchant.Name);
            
            if(existingMerchant == null)
                throw new MerchantNotFoundException(merchant.Name);

            Mapper.Map(merchant, existingMerchant);
            
            await _merchantRepository.ReplaceAsync(existingMerchant);
            
            await _log.WriteInfoAsync(nameof(MerchantService), nameof(UpdateAsync),
                merchant.ToContext(),
                "Merchant updated");
        }

        public async Task SetPublicKeyAsync(string merchantName, string publicKey)
        {
            IMerchant merchant = await _merchantRepository.GetAsync(merchantName);
            
            if(merchant == null)
                throw new MerchantNotFoundException(merchantName);
            
            merchant.PublicKey = publicKey;
            
            await _merchantRepository.ReplaceAsync(merchant);
            
            await _log.WriteInfoAsync(nameof(MerchantService), nameof(SetPublicKeyAsync),
                merchant.ToContext(),
                "Merchant public key updated");
        }

        public async Task DeleteAsync(string merchantName)
        {
            await _merchantRepository.DeleteAsync(merchantName);
            
            await _log.WriteInfoAsync(nameof(MerchantService), nameof(DeleteAsync),
                new{MerchantId = merchantName}.ToJson(),
                "Merchant deleted");
        }
    }
}
