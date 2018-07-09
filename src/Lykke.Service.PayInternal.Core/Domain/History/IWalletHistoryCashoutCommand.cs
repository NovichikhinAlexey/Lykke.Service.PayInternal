namespace Lykke.Service.PayInternal.Core.Domain.History
{
    public interface IWalletHistoryCashoutCommand : IWalletHistoryCommand
    {
        string DesiredAsset { get; set; }
        string EmployeeEmail { get; set; }
    }
}
