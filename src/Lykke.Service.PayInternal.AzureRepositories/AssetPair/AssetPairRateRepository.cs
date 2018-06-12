using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain.AssetPair;

namespace Lykke.Service.PayInternal.AzureRepositories.AssetPair
{
    public class AssetPairRateRepository : IAssetPairRateRepository
    {
        private readonly INoSQLTableStorage<AssetPairRateEntity> _tableStorage;

        public AssetPairRateRepository([NotNull] INoSQLTableStorage<AssetPairRateEntity> tableStorage)
        {
            _tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
        }

        public async Task<IAssetPairRate> AddAsync(IAssetPairRate src)
        {
            AssetPairRateEntity entity = AssetPairRateEntity.ByDate.Create(src);

            await _tableStorage.InsertAsync(entity);

            return Mapper.Map<AssetPairRate>(entity);
        }

        public async Task<IReadOnlyList<IAssetPairRate>> GetByDateAsync(DateTime date)
        {
            IEnumerable<AssetPairRateEntity> rates =
                await _tableStorage.GetDataAsync(AssetPairRateEntity.ByDate.GeneratePartitionKey(date));

            return Mapper.Map<IReadOnlyList<AssetPairRate>>(rates);
        }

        public async Task<IReadOnlyList<IAssetPairRate>> GetAsync(string baseAssetId, string quotingAssetId)
        {
            IList<AssetPairRateEntity> rates = await _tableStorage.GetDataAsync(e =>
                e.BaseAssetId == baseAssetId && e.QuotingAssetId == quotingAssetId);

            return Mapper.Map<IReadOnlyList<AssetPairRate>>(rates);
        }
    }
}
