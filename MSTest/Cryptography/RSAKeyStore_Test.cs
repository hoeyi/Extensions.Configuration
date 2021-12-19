using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Ichsoft.Configuration.Extensions.Cryptography;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;


namespace MSTest.Cryptography
{
    /// <summary>
    /// Past containers:
    ///     EulerFinancial{1.0}
    /// </summary>
    [TestClass]
    public class RSAKeyStore_Test
    {
        private readonly string keyContainer = "MSTest.Cryptography{1.0}";
        private readonly IReadOnlyDictionary<string, string> testStrings =
            new Dictionary<string, string>()
            {
                {
                    "f8mTvHFUNVFq4E5vBQbTOj5YM4pXUNCgt1ZLlPPcz8WhXddNG1+nxiiyE6O0xW4AffhzugtIwvZPBH90A8IGzgCQbKUhdPotooGA5GDP3JUEqT7bS6+bZyN3zEoFuPk50TIhqNysbUgEa3HXH3/iWQENzhjffjI9vYtAQLFEXuc=",
                    "Unencrypted string" 
                },
                {
                    "uL/LFZkSCBDNVhVKviQSnplTReQvHgHqQrXvBL5gbjGAd55C/vyGMvmg/pCWNDDw9vsk8JQvwGRWYNqxfgWgN3ViVSpvepvRQu7ZoSd8s2OZtMG26rPzhl0DWyKaBFCQ/gNsSwHIsQXLRCvWO/t/ybaEc40OJeBnPME2qiAxbu0=",
                    "Unencrypted string"
                }
            };

        [TestMethod] 
        public void Delete_ValidContainer_ReturnsTrue()
        {
            var keyStore = new RSAKeyStore(containerName: keyContainer, logger: MSTest.Logger);

            Assert.IsTrue(keyStore.DeleteKeyFromContainer(containerName: keyContainer));
        }

        [TestMethod]
        public void Encrypt_ValidContainer_ReturnsTrue()
        {
            var keyStore = new RSAKeyStore(containerName: keyContainer, logger: MSTest.Logger);

            foreach(var kv in testStrings)
            {
                string cipherText = keyStore.Encrypt(kv.Value);
                string plainText = keyStore.Decrypt(cipherText);

                Debug.WriteLine($"Plain text: {kv.Value}");
                Debug.WriteLine($"Cipher: {cipherText}");
                Debug.WriteLine($"Decrypted: {plainText}");

                Assert.IsInstanceOfType(cipherText, typeof(string));
                Assert.AreEqual(expected: kv.Value, actual: plainText);
            }
        }

        [TestMethod]
        public void Decrypt_ValidContainer_ReturnsTrue()
        {
            var keyStore = new RSAKeyStore(containerName: keyContainer, logger: MSTest.Logger);
            foreach(var kv in testStrings)
            {
                string plainText = keyStore.Decrypt(kv.Key);
                Debug.WriteLine($"Cipher text: {kv.Key}\nPlain text: {plainText}");

                Assert.AreEqual(expected: kv.Value, actual: plainText);
            }
        }

        [TestMethod]
        public void Create_ValideContainer_ReturnsTrue()
        {
            var keyStore = new RSAKeyStore(containerName: keyContainer, logger: MSTest.Logger);
            Assert.IsInstanceOfType(value: keyStore, expectedType: typeof(RSAKeyStore));
        }
    }
}
