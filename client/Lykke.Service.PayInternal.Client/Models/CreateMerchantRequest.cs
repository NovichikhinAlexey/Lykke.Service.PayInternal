namespace Lykke.Service.PayInternal.Client.Models
{
    public class CreateMerchantRequest
    {
        public string Name { get; set; }

        public string ApiKey { get; set; }

        public double DeltaSpread { get; set; }

        public int TimeCacheRates { get; set; }

        public double LpMarkupPercent { get; set; }

        public int LpMarkupPips { get; set; }

        public string LwId { get; set; }
    }
}
