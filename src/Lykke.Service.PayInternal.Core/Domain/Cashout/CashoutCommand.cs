namespace Lykke.Service.PayInternal.Core.Domain.Cashout
{
    public class CashoutCommand
    {
        public string MerchantId { get; set; }
        public string EmployeeEmail { get; set; }
        public string SourceMerchantWalletId { get; set; }
        public string SourceAssetId { get; set; }
        public string DesiredAsset { get; set; }
        public decimal SourceAmount { get; set; }
    }
}
