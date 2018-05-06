namespace Lykke.Service.PayInternal.Models
{
    public class MerchantModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string PublicKey { get; set; }

        public string ApiKey { get; set; }

        public int TimeCacheRates { get; set; }
        
        public string LwId { get; set; }
    }
}
