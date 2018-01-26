namespace Lykke.Service.PayInternal.Models
{
    public class MerchantModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string PublicKey { get; set; }

        public string ApiKey { get; set; }

        public double DeltaSpread { get; set; }

        public int TimeCacheRates { get; set; }

        public double LpMarkupPercent { get; set; }

        public int LpMarkupPips { get; set; }

        public string LwId { get; set; }
    }
}
