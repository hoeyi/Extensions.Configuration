using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ichsoft.Configuration.Extensions
{
    class EncryptedConfiguration : ConfigurationProvider
    {
        public readonly ICryptoTransform Decryptor;
        public readonly ICryptoTransform Encryptor;
        internal EncryptedConfiguration(byte[] Key, byte[] IV)
        {
            Aes aes = Aes.Create();
            Decryptor = aes.CreateDecryptor(Key, IV);
            Encryptor = aes.CreateEncryptor(Key, IV);
        }

        public override bool TryGet(string key, out string value)
        {
            if (base.TryGet(key, out value))
            {
                byte[] decryptedBytes = Convert.FromBase64String(value);
                byte[] textBytes = Decryptor.TransformFinalBlock(decryptedBytes, 0, decryptedBytes.Length);
                value = Encoding.Unicode.GetString(textBytes);
                return true;
            }
            return false;
        }

        public override void Set(string key, string value)
        {
            byte[] textBytes = Encoding.Unicode.GetBytes(value);
            byte[] decryptedBytes = Decryptor.TransformFinalBlock(textBytes, 0, textBytes.Length);
            base.Set(key, Convert.ToBase64String(decryptedBytes));
        }
    }
}
