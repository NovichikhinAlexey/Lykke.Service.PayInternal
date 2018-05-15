namespace Lykke.Service.PayInternal.Client.Models.Markup
{
    public class UpdateMarkupRequest
    {
        public decimal DeltaSpread { get; set; }

        public decimal Percent { get; set; }

        public int Pips { get; set; }

        public decimal FixedFee { get; set; }
    }
}
