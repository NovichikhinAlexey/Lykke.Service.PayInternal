using System;
using Lykke.AzureStorage.Tables.Entity.Serializers;

namespace Lykke.Service.PayInternal.AzureRepositories.Serializers
{
    public class StringListSerializer : IStorageValueSerializer
    {
        private const char Delimiter = ';';

        public string Serialize(object value)
        {
            if (!(value is string[] sources))
                throw new Exception("Source is not a string array");

            var serialized = string.Join(Delimiter, sources);

            return serialized;
        }

        public object Deserialize(string serialized)
        {
            string[] result = serialized.Split(Delimiter);

            return result;
        }
    }
}
