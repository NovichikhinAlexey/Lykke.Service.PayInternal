using System.ComponentModel.DataAnnotations;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Validation
{
    public class AssetExistsAttribute : ValidationAttribute
    {
        private const string Message = "Asset not found";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var lykkeAssetsResolver = (ILykkeAssetsResolver) validationContext.GetService(typeof(ILykkeAssetsResolver));

            string lykkeAssetId;

            try
            {
                string assetId = (string) value;

                if (string.IsNullOrEmpty(assetId))
                    return ValidationResult.Success;

                lykkeAssetId = lykkeAssetsResolver?.GetLykkeId(assetId).GetAwaiter().GetResult();
            }
            catch (AssetUnknownException)
            {
                return new ValidationResult(Message);
            }

            var assetsLocalCache = (IAssetsLocalCache) validationContext.GetService(typeof(IAssetsLocalCache));

            Asset asset = assetsLocalCache?.GetAssetByIdAsync(lykkeAssetId).GetAwaiter().GetResult();

            if (asset == null)
                return new ValidationResult(Message);

            return ValidationResult.Success;
        }
    }
}
