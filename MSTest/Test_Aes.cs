using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Hoeyi.Extensions.Configuration.Cryptography;
using System.Diagnostics;

namespace MSTest
{
    [TestClass]
    public class Test_Aes
    {
        [TestMethod]
        public void EncryptString_DefaultAes_YieldsString()
        {
            var plainText = "Encrypt this string";


            using var aes = Aes.Create();
            aes.GenerateKey();

            var cipherText = AesWorker.Encrypt(
                plainText: plainText,
                aesKey: Convert.ToBase64String(aes.Key),
                out string aesIV);

            Debug.WriteLine($"Plain: {plainText}");
            Debug.WriteLine($"Encrypted: {cipherText}");
            Debug.WriteLine($"IV: {aesIV}");

            Assert.IsInstanceOfType(cipherText, typeof(string));
        }

        [TestMethod]
        public void EncryptDecrypt_DefaultAes_YieldMatchingString()
        {
            var plainText = "Encrypt this string";


            using var aes = Aes.Create();
            aes.GenerateKey();

            var cipherText = AesWorker.Encrypt(
                plainText: plainText,
                aesKey: Convert.ToBase64String(aes.Key),
                out string aesIV);

            var decryptedText = AesWorker.Decrypt(
                cipherText: cipherText,
                aesKey: Convert.ToBase64String(aes.Key),
                aesIV: aesIV);

            Debug.WriteLine($"Plain: {plainText}");
            Debug.WriteLine($"Encrypted: {cipherText}");
            Debug.WriteLine($"IV: {aesIV}");
            Debug.WriteLine($"Decrypted: {decryptedText}");

            Assert.AreEqual(plainText, decryptedText);
        }
    }
}
