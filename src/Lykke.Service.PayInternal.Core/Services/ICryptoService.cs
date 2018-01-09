namespace Lykke.Service.PayInternal.Core.Services
{
    public interface ICryptoService
    {
        string Encrypt(string src);
        string Decrypt(string src);
    }
}
