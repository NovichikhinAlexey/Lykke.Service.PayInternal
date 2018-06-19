using System;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Mapping
{
    public class AssetIdValueResolver : IMemberValueResolver<object, object, string, string>
    {
        private readonly ILykkeAssetsResolver _lykkeAssetsResolver;

        public AssetIdValueResolver(
            [NotNull] ILykkeAssetsResolver lykkeAssetsResolver)
        {
            _lykkeAssetsResolver = lykkeAssetsResolver ?? throw new ArgumentNullException(nameof(lykkeAssetsResolver));
        }

        public string Resolve(object source, object destination, string sourceMember, string destMember, ResolutionContext context)
        {
            return _lykkeAssetsResolver.GetLykkeId(sourceMember).GetAwaiter().GetResult();
        }
    }
}
