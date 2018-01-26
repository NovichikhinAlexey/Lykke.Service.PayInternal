namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public enum TransferStatusError
    {
        NotError = 0,
        NotConsermed,
        InvalidAmount,
        InvalidAddress,
        InternalError
    }
}
