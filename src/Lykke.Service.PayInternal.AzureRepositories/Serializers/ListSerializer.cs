using System;
using System.Collections.Generic;
using Common;
using Lykke.AzureStorage.Tables.Entity.Serializers;
using Newtonsoft.Json;

namespace Lykke.Service.PayInternal.AzureRepositories.Serializers
{
    public class ListSerializer<T> : IStorageValueSerializer
    {
        public string Serialize(object value)
        {
            if (!(value is T[]))
                throw new Exception($"Source is not an array of {typeof(T).Name}");

            var serialized = value.ToJson();

            return serialized;
        }

        public object Deserialize(string serialized)
        {
            return JsonConvert.DeserializeObject<List<T>>(serialized);
        }
    }
}
