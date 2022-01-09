using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Ichosoft.Extensions.Configuration.Cryptography
{
    /// <summary>
    /// Contains methods for working with AES encryption.
    /// </summary>
    static class AESProvider
    {
        /// <summary>
        /// Generates a new symmetric key for encryption and decryption.
        /// </summary>
        /// <param name="keySize">The size in bits for the generated key. Valid 
        /// AES values are 128, 192, 256. The default is 256.</param>
        /// <returns>The secret key with length of <paramref name="keySize"/> bits.</returns>
        public static string GenerateKey(int keySize = 256)
        {
            // Check given size is valid.
            var validSizes = new int[] { 128, 192, 256 };
            if (!validSizes.Contains(keySize))
                throw new InvalidOperationException(
                    string.Format(Resources.ExceptionString.Aes_InvalidSize, keySize));

            // Generate the secret key.
            using Aes aes = Aes.Create();
            aes.KeySize = keySize;
            aes.GenerateKey();

            // Return as base-64 string.
            return Convert.ToBase64String(aes.Key);
        }

        /// <summary>
        /// Encrypts the input string using the provided key.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <param name="aesKey">The symmetric key that is used for encryption and decryption.</param>
        /// <param name="aesIV">The IV generated when the data was encrypted.</param>
        /// <returns>The encrypted text.</returns>
        public static string Encrypt(
            string plainText, 
            string aesKey, 
            out string aesIV)
        {
            byte[] cipher = Encrypt(
                plainText: plainText,
                aesKey: Convert.FromBase64String(aesKey),
                out byte[] iv);

            aesIV = Convert.ToBase64String(iv);

            return Convert.ToBase64String(cipher);
        }

        /// <summary>
        /// Decrypts the input string using the provided key and IV.
        /// </summary>
        /// <param name="cipherText">The encrypted text.</param>
        /// <param name="aesKey">The symmetric key that is used for encryption and decryption.</param>
        /// <param name="aesIV">The IV generated when the data was encrypted.</param>
        /// <returns>The decrypted text.</returns>
        public static string Decrypt(string cipherText, string aesKey, string aesIV)
        {
            return Decrypt(
                cipher: Convert.FromBase64String(cipherText),
                aesKey: Convert.FromBase64String(aesKey),
                aesIV: Convert.FromBase64String(aesIV));
        }

        /// <summary>
        /// Decrypts the input string with prepended IV using the given key.
        /// </summary>
        /// <param name="cipherTextWithIV">The cipher text with the IV prepended.</param>
        /// <param name="aesKey">The symmetric key that is used for encryption and decryption.</param>
        /// <returns>The decrypted text.</returns>
        public static string Decrypt(string cipherTextWithIV, string aesKey)
        {
            return Decrypt(
                cipher: Convert.FromBase64String(cipherTextWithIV[24..]),
                aesKey: Convert.FromBase64String(aesKey),
                aesIV: Convert.FromBase64String(cipherTextWithIV.Substring(0, 24)));
        }

        /// <summary>
        /// Encrypts the input data using the provided key.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <param name="aesKey">The symmetric key that is used for encryption and decryption.</param>
        /// <param name="aesIV">The IV generated when the data was encrypted.</param>
        /// <returns>The encrypted text as a byte array.</returns>
        private static byte[] Encrypt(string plainText, byte[] aesKey, out byte[] aesIV)
        {
            using Aes aes = Aes.Create();
            aes.Key = aesKey;
            aes.GenerateIV();
            aesIV = aes.IV;
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

            return encrypted;          
        }

        /// <summary>
        /// Decrypts the input data using the provided key and IV.
        /// </summary>
        /// <param name="cipher">The data to decrypt.</param>
        /// <param name="aesKey">The symmetric key that is used for encryption and decryption.</param>
        /// <param name="aesIV">The IV generated when the data was encrypted.</param>
        /// <returns>The decrypted data as a string.</returns>
        private static string Decrypt(byte[] cipher, byte[] aesKey, byte[] aesIV)
        {
            using AesCryptoServiceProvider aes = new();
            aes.Key = aesKey;
            aes.IV = aesIV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ICryptoTransform decryptor = aes.CreateDecryptor();

            // Create the streams used for decryption.
            using MemoryStream msDecrypt = new(cipher);
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);

            using StreamReader stringDecrypt = new(csDecrypt);
            return stringDecrypt.ReadToEnd();
        }
    }
}
