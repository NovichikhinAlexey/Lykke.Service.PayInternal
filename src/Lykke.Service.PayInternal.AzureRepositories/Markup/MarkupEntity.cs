using System;
using AutoMapper;
using AzureStorage.Tables.Templates.Index;
using Common;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Markup;
using Lykke.Service.PayInternal.Core.Exceptions;

namespace Lykke.Service.PayInternal.AzureRepositories.Markup
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class MarkupEntity : AzureTableEntity
    {
        private decimal _deltaSpread;
        private decimal _percent;
        private int _pips;
        private decimal _fixedFee;
        private MarkupIdentityType _markupIdentityType;
        private PriceMethod _priceMethod;
        private DateTime _createdOn;

        public decimal DeltaSpread
        {
            get => _deltaSpread;
            set
            {
                _deltaSpread = value;
                MarkValueTypePropertyAsDirty(nameof(DeltaSpread));
            }
        }

        public decimal Percent
        {
            get => _percent;
            set
            {
                _percent = value;
                MarkValueTypePropertyAsDirty(nameof(Percent));
            }
        }

        public int Pips
        {
            get => _pips;
            set
            {
                _pips = value;
                MarkValueTypePropertyAsDirty(nameof(Pips));
            }
        }

        public decimal FixedFee
        {
            get => _fixedFee;
            set
            {
                _fixedFee = value;
                MarkValueTypePropertyAsDirty(nameof(FixedFee));
            }
        }

        public string AssetPairId { get; set; }

        public string PriceAssetPairId { get; set; }

        public PriceMethod PriceMethod
        {
            get => _priceMethod;
            set
            {
                _priceMethod = value;
                MarkValueTypePropertyAsDirty(nameof(PriceMethod));
            }
        }

        public MarkupIdentityType IdentityType
        {
            get => _markupIdentityType;
            set
            {
                _markupIdentityType = value;
                MarkValueTypePropertyAsDirty(nameof(IdentityType));
            }
        }

        public DateTime CreatedOn
        {
            get => _createdOn;
            set
            {
                _createdOn = value;
                MarkValueTypePropertyAsDirty(nameof(CreatedOn));
            }
        }

        public string Identity { get; set; }

        public static class ByAssetPair
        {
            public static string GeneratePartitionKey(string assetPairId)
            {
                if (!assetPairId.IsValidPartitionOrRowKey())
                    throw new InvalidRowKeyValueException(nameof(assetPairId), assetPairId);

                return assetPairId;
            }

            public static string GenerateRowKey(MarkupIdentityType identityType, string identity)
            {
                string key = identityType == MarkupIdentityType.None
                    ? $"{identityType.ToString()}"
                    : $"{identityType.ToString()}_{identity}";

                if (!key.IsValidPartitionOrRowKey())
                    throw new InvalidRowKeyValueException(nameof(key), key);

                return key;
            }

            public static MarkupEntity Create(IMarkup src)
            {
                var entity = new MarkupEntity
                {
                    PartitionKey = GeneratePartitionKey(src.AssetPairId),
                    RowKey = GenerateRowKey(src.IdentityType, src.Identity)
                };

                return Mapper.Map(src, entity);
            }
        }

        public static class IndexByIdentity
        {
            public static string GeneratePartitionKey(MarkupIdentityType identityType, string identity)
            {
                string key = identityType == MarkupIdentityType.None
                    ? $"{identityType.ToString()}"
                    : $"{identityType.ToString()}_{identity}";

                if (!key.IsValidPartitionOrRowKey())
                    throw new InvalidRowKeyValueException(nameof(key), key);

                return key;
            }

            public static string GenerateRowKey(string assetPairId)
            {
                if (!assetPairId.IsValidPartitionOrRowKey())
                    throw new InvalidRowKeyValueException(nameof(assetPairId), assetPairId);

                return assetPairId;
            }

            public static AzureIndex Create(MarkupEntity entity)
            {
                return AzureIndex.Create(
                    GeneratePartitionKey(entity.IdentityType, entity.Identity),
                    GenerateRowKey(entity.AssetPairId), entity);
            }
        }
    }
}
