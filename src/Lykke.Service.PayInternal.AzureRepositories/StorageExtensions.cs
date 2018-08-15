using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Exceptions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Protocol;

namespace Lykke.Service.PayInternal.AzureRepositories
{
    public static class StorageExtensions
    {
        public static async Task InsertThrowConflict<T>(this INoSQLTableStorage<T> storage, T entity) where T : ITableEntity, new()
        {
            const int conflict = 409;

            try
            {
                await storage.InsertAsync(entity, conflict);
            }
            catch (StorageException exception)
            {
                if (exception.RequestInformation != null &&
                    exception.RequestInformation.HttpStatusCode == conflict &&
                    exception.RequestInformation.ExtendedErrorInformation.ErrorCode == TableErrorCodeStrings.EntityAlreadyExists)
                {
                    throw new DuplicateKeyException("Entity already exists", exception);
                }

                throw;
            }
        }

        public static async Task DeleteThrowNotFound(this IBlobStorage storage, string container, string key)
        {
            const int notFound = 404;

            try
            {
                await storage.DelBlobAsync(container, key);
            }
            catch (StorageException exception)
            {
                if (exception.RequestInformation != null &&
                    exception.RequestInformation.HttpStatusCode == notFound &&
                    exception.RequestInformation.ExtendedErrorInformation.ErrorCode == "BlobNotFound")
                {
                    throw new KeyNotFoundException("Blob not found", exception);
                }

                throw;
            }
        }
    }
}
