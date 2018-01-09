using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Lykke.Service.PayInternal.Core.Services;

namespace Lykke.Service.PayInternal.Services
{
    public class CryptoService : ICryptoService
    {
        private readonly byte[] _sha256Hash;
        private readonly byte[] _md5Hash;

        public CryptoService(string key)
        {
            var keyInBytes = Encoding.UTF8.GetBytes(key);

            using (var sha256 = SHA256.Create())
            {
                _sha256Hash = sha256.ComputeHash(keyInBytes);
            }

            using (var md5 = MD5.Create())
            {
                _md5Hash = md5.ComputeHash(keyInBytes);
            }
        }

        public string Decrypt(string src)
        {
            byte[] result;
            using (var aes = Aes.Create())
            {
                aes.Key = _sha256Hash;
                aes.IV = _md5Hash;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var resultStream = new MemoryStream())
                {
                    using (var aesStream = new CryptoStream(resultStream, decryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(Convert.FromBase64String(src)))
                    {
                        plainStream.CopyTo(aesStream);
                    }

                    result = resultStream.ToArray();
                }
            }

            return Encoding.UTF8.GetString(result);
        }

        public string Encrypt(string src)
        {
            byte[] result;
            using (var aes = Aes.Create())
            {
                aes.Key = _sha256Hash;
                aes.IV = _md5Hash;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var resultStream = new MemoryStream())
                {
                    using (var aesStream = new CryptoStream(resultStream, encryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(Encoding.UTF8.GetBytes(src)))
                    {
                        plainStream.CopyTo(aesStream);
                    }

                    result = resultStream.ToArray();
                }
            }

            return Convert.ToBase64String(result);
        }
    }
}
