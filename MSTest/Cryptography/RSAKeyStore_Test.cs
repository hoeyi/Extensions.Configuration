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
    /// EulerFinancial{1.0}
    /// </summary>
    [TestClass]
    public class RSAKeyStore_Test
    {
        private readonly string keyContainer = "EulerFinancial{1.0}";
        private readonly IReadOnlyDictionary<string, string> testStrings =
            new Dictionary<string, string>()
            {
                {
                    "sloFchfdAFXV1+j46THndWKzXr4DnvOGRh2fyuaErud92Plbx5WjbScCe4dXHluQDGsXhqZghHcJnp+8weKFXfcsX0Ko2mdw6HXHXt6duofBhGjuY2ErHVmhVJCfEKA6k34hsr3gVh7cniXIz7iTLiGedzg9EUC+m8hsRUQH2W0=",
                    "Unencrypted string" 
                },
                {
                    "bTWnpLpm0lpZZB9SDs3x/PxOtKSoGHT2LsNcISCzbicVyVuo8bxNNJFNsBYz82Ubp9j5OMpGhJrD45eMjv4gQYAPHOIQf3/g1uytBovIJ6N/6upyJE2yaXLA9fTIKvBw7IbZDrG/0p+G8wO0dRL0XFj1mPVnC1vxOD/hmVTsRpI=",
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
