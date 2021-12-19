using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Ichsoft.Configuration.Extensions.Cryptography;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;


namespace MSTest.Cryptography
{
    [TestClass]
    public class RSAKeyStore_Test
    {
        private readonly string keyContainer = "EulerFinancial{1.0}";
        private readonly IReadOnlyDictionary<string, string> testStrings =
            new Dictionary<string, string>()
            {
                { "Unencrypted string", "SAi7B/or9QeIkX6z5fWkiF1Zah57gRa2thiWwwTsA6Mtg/NMn/azbL6ltdB228WxcXNoQVtJrU+UFYnaJf+5qnI6xXU7XMSsEi7WzJIlkC0RIwJH+REVtqIIwllt56YTLag/GheWZ8ZmiawYY8Echvw0Uw2b57MAv+p6jCyBixo=" }
            };

        [TestMethod] 
        public void KeyStore_DeleteKeyContainer_ReturnsTrue()
        {
            var keyStore = new RSAKeyStore(containerName: keyContainer, logger: MSTest.Logger);

            //Assert.IsTrue(keyStore.DeleteKeyFromContainer(containerName: keyContainer));
        }

        [TestMethod]
        public void Encrypt_ValidContainer_ReturnsTrue()
        {
            var keyStore = new RSAKeyStore(containerName: keyContainer, logger: MSTest.Logger);

            foreach(var kv in testStrings)
            {
                string cipherText = keyStore.Encrypt(kv.Value);

                Debug.WriteLine($"Cipher: {cipherText}");
                Debug.WriteLine($"Decrypted: {keyStore.Decrypt(cipherText)}");

                Assert.IsInstanceOfType(cipherText, typeof(string));
                //Assert.AreEqual(expected: kv.Key, actual: plainText);
            }
        }

        [TestMethod]
        public void Decrypt_ValidContainer_ReturnsTrue()
        {
            var keyStore = new RSAKeyStore(containerName: keyContainer, logger: MSTest.Logger);
            foreach(var kv in testStrings)
            {
                string plainText = keyStore.Decrypt(testStrings[kv.Key]);
                Debug.WriteLine($"Plain text: {kv.Key}\nCipher text: {kv.Value}");

                Assert.AreEqual(expected: kv.Key, actual: plainText);
            }
        }

        [TestMethod]
        public void Test()
        {
            if (!OperatingSystem.IsWindows())
                return;

            var cspParams = new CspParameters()
            {
                KeyContainerName = "Test",
                Flags = CspProviderFlags.NoPrompt
            };

            using var rsa = new RSACryptoServiceProvider(cspParams);

            var plainText = "Unencrypted string";
            var cipherTextObs = testStrings[plainText];
            var cipherTextExp = SecureOptions.Encrypt(rsa, plainText);

            Debug.Write($"Expected: {cipherTextExp}\nObserved: {cipherTextObs}");
        }
    }
}
