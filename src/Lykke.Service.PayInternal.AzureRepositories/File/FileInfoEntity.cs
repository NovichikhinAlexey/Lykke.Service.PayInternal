using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.PayInternal.AzureRepositories.File
{
    public class FileInfoEntity : TableEntity
    {
        public FileInfoEntity()
        {
        }

        public FileInfoEntity(string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        {
        }

        public string MerchantId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Size { get; set; }
    }
}
