using System;
using AutoMapper;
using Common;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Mapping
{
    public class AssetDisplayIdValueResolver : IMemberValueResolver<object, object, string, string>
    {
        private readonly IAssetsLocalCache _assetsLocalCache;

        public AssetDisplayIdValueResolver(
            [NotNull] IAssetsLocalCache assetsLocalCache)
        {
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
        }

        public string Resolve(object source, object destination, string sourceMember, string destMember, ResolutionContext context)
        {
            if (!sourceMember.IsGuid())
                return sourceMember;

            return _assetsLocalCache.GetAssetByIdAsync(sourceMember).GetAwaiter().GetResult().DisplayId;
        }
    }
}
