using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hoeyi.Extensions.Configuration.Cryptography;

namespace Extensions.Configuration.Test
{
    [TestClass]
    public class SymmetricEncryption
    {
        [TestMethod]
        public void AesEncrypt_String_YieldsString()
        {
            var plainText = "Encrypt this string";


            string aesKey = AesWorker.GenerateKey(keySize: 256);

            var cipherText = AesWorker.Encrypt(
                plainText: plainText,
                aesKey: aesKey,
                out string aesIV);

            Reference.Logger.LogDebug(
                $"Plain: {plainText}" +
                $"\nEncrypted: {cipherText}" +
                $"\nIV: {aesIV}");

            Assert.IsInstanceOfType(cipherText, typeof(string));
        }

        [TestMethod]
        public void AesEncryptDecrypt_String_YieldsOriginalString()
        {
            var plainText = "Encrypt this string";

            string aesKey = AesWorker.GenerateKey(keySize: 256);

            var cipherText = AesWorker.Encrypt(
                plainText: plainText,
                aesKey: aesKey,
                out string aesIV);

            var decryptedText = AesWorker.Decrypt(
                cipherText: cipherText,
                aesKey: aesKey,
                aesIV: aesIV);

            Reference.Logger.LogDebug(
                $"Plain: {plainText}" +
                $"\nEncrypted: {cipherText}" +
                $"\nIV: {aesIV}" +
                $"\nDecrypted: {decryptedText}");

            Assert.AreEqual(plainText, decryptedText);
        }

        [TestMethod]
        public void AesEncrypt_String_MultipleCalls_YieldsDifferentCipherAndIV()
        {
            var plainText = "Encrypt this string";

            string aesKey = AesWorker.GenerateKey(keySize: 256);

            var cipher1 = AesWorker.Encrypt(
                plainText: plainText,
                aesKey: aesKey,
                out string iv1);

            var cipher2 = AesWorker.Encrypt(
                plainText: plainText,
                aesKey: aesKey,
                out string iv2);

            Assert.AreNotEqual(cipher1, cipher2);
            Assert.AreNotEqual(iv1, iv2);
        }
    }
}
