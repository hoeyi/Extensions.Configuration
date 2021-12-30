using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hoeyi.Extensions.Configuration.Cryptography
{
    /// <summary>
    /// Contains methods for working with AES encryption.
    /// </summary>
    static class AesWorker
    {
        /// <summary>
        /// Generates a new AES secret key of the given size. 
        /// </summary>
        /// <param name="keySize">The size in bits for the generated key. Valid 
        /// AES values are 128, 192, 256. The default is 256.</param>
        /// <returns></returns>
        public static string GenerateKey(int? keySize)
        {
            // Check given size is valid.
            int kSize = keySize ?? 256;
            var validSizes = new int[] { 128, 192, 256 };
            if (!validSizes.Contains(kSize))
                throw new InvalidOperationException(
                    string.Format(Resources.ExceptionString.Aes_InvalidSize, kSize));

            // Generate the secret key.
            using Aes aes = Aes.Create();
            aes.KeySize = kSize;
            aes.GenerateKey();

            // Return as base-64 string.
            return Convert.ToBase64String(aes.Key);
        }

        /// <summary>
        /// Encrypts text using a given AES secret key.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <param name="aesKey">The key to use..</param>
        /// <param name="aesIV">The randomly generated IV used during encryption.</param>
        /// <returns>The given plain text string as cipher text.</returns>
        public static string Encrypt(string plainText, string aesKey, out string aesIV)
        {
            using Aes aes = Aes.Create();
            aes.Key = Convert.FromBase64String(aesKey);
            aes.GenerateIV();
            aesIV = Convert.ToBase64String(aes.IV);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            // Create the streams used for encryption.
            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            using (StreamWriter streamWriter = new(cryptoStream))
            {
                streamWriter.Write(plainText);
            }

            // Read the encrypted bytes from the stream and place in array.
            byte[] encrypted = memoryStream.ToArray();

            // Retun the byte array as a base-64 string.
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Decrypts textusing a given AES secret key.
        /// </summary>
        /// <param name="cipherText">The encrypted text.</param>
        /// <param name="aesKey">The key used to encrypt the text.</param>
        /// <param name="aesIV">The IV used when encrypting the text.</param>
        /// <returns>The given cipher text as plain text.</returns>
        public static string Decrypt(string cipherText, string aesKey, string aesIV)
        {
            using AesCryptoServiceProvider aes = new();
            aes.Key = Convert.FromBase64String(aesKey);
            aes.IV = Convert.FromBase64String(aesIV);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ICryptoTransform decryptor = aes.CreateDecryptor();

            // Create the streams used for decryption.
            using MemoryStream msDecrypt = new(Convert.FromBase64String(cipherText));
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new(csDecrypt);

            // Read the decrypted bytes from the decrypting stream
            // and place them in a string.
            return srDecrypt.ReadToEnd();
        }
    }
}
