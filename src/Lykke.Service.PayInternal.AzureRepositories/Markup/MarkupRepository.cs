using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Markup;

namespace Lykke.Service.PayInternal.AzureRepositories.Markup
{
    public class MarkupRepository : IMarkupRepository
    {
        private readonly INoSQLTableStorage<MarkupEntity> _tableStorage;
        private readonly INoSQLTableStorage<AzureIndex> _indexByIdentity;

        public MarkupRepository(
            INoSQLTableStorage<MarkupEntity> tableStorage, 
            INoSQLTableStorage<AzureIndex> indexByIdentity)
        {
            _tableStorage = tableStorage;
            _indexByIdentity = indexByIdentity;
        }

        public async Task<IMarkup> SetAsync(IMarkup markup)
        {
            MarkupEntity entity = MarkupEntity.ByAssetPair.Create(markup);

            await _tableStorage.InsertOrReplaceAsync(entity);

            AzureIndex index = MarkupEntity.IndexByIdentity.Create(entity);

            await _indexByIdentity.InsertOrReplaceAsync(index);

            return Mapper.Map<Core.Domain.Markup.Markup>(entity);
        }

        public async Task<IReadOnlyList<IMarkup>> GetByIdentityAsync(MarkupIdentityType identityType, string identity)
        {
            IEnumerable<AzureIndex> indecies =
                await _indexByIdentity.GetDataAsync(
                    MarkupEntity.IndexByIdentity.GeneratePartitionKey(identityType, identity));

            IEnumerable<MarkupEntity> entities = await _tableStorage.GetDataAsync(indecies);

            return Mapper.Map<IEnumerable<Core.Domain.Markup.Markup>>(entities).ToList();
        }

        public async Task<IMarkup> GetByIdentityAsync(MarkupIdentityType identityType, string identity, string assetPairId)
        {
            AzureIndex index = await _indexByIdentity.GetDataAsync(
                MarkupEntity.IndexByIdentity.GeneratePartitionKey(identityType, identity),
                MarkupEntity.IndexByIdentity.GenerateRowKey(assetPairId));

            if (index == null) return null;

            MarkupEntity entity = await _tableStorage.GetDataAsync(index);

            return Mapper.Map<Core.Domain.Markup.Markup>(entity);
        }

        public async Task<IReadOnlyList<IMarkup>> GetByAssetAsync(string assetPairId)
        {
            IEnumerable<MarkupEntity> entities =
                await _tableStorage.GetDataAsync(MarkupEntity.ByAssetPair.GeneratePartitionKey(assetPairId));

            return Mapper.Map<IEnumerable<Core.Domain.Markup.Markup>>(entities).ToList();
        }
    }
}
