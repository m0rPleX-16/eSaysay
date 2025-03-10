using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace eSaysay.Services
{
    public class EncryptionService
    {
        private readonly ILogger<EncryptionService> _logger;
        private readonly string _encryptionKey;

        public EncryptionService(IConfiguration configuration, ILogger<EncryptionService> logger)
        {
            _logger = logger;
            _encryptionKey = configuration["AppSettings:EncryptionKey"] ?? throw new Exception("Encryption key must be exactly 32 characters.");

            if (string.IsNullOrEmpty(_encryptionKey) || _encryptionKey.Length != 32)
            {
                throw new Exception("Encryption key must be exactly 32 characters.");
            }
        }

        public string EncryptData(string plainText)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText)) return string.Empty;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
                    aes.IV = new byte[16];

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Encryption] Error encrypting data: {ex.Message}");
                return null;
            }
        }

        public string DecryptData(string encryptedData)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedData)) return string.Empty;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
                    aes.IV = new byte[16];

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream(Convert.FromBase64String(encryptedData)))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Decryption] Error decrypting data: {ex.Message}");
                return null;
            }
        }
    }
}
