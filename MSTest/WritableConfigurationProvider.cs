using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hoeyi.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Collections.Generic;

namespace MSTest
{
    /// <summary>
    /// Contains tests for the behavior of writable and encrytable configuration files.
    /// </summary>
    [TestClass]
    public class WritableConfigurationProvider
    {
        /// <summary>
        /// Verifies a writable configuration can be constructed.
        /// </summary>
        [TestMethod]
        public void InitConfiguration_ValidFile_YieldsInstance()
        {
            var config = GetPlainTextConfiguration();

            Assert.IsInstanceOfType(config, typeof(IConfigurationRoot));
        }

        /// <summary>
        /// Verifies a writable configuration with encrypted values can be constructed.
        /// </summary>
        [TestMethod]
        public void InitConfigurationEncrypted_ValidFile_YieldsInstance()
        {
            var config = GetEncryptedConfiguration();

            Assert.IsInstanceOfType(config, typeof(IConfigurationRoot));
        }

        /// <summary>
        /// Verifies setting an unencrypted value is committed to file and accessible 
        /// to subsequent loads.
        /// </summary>
        [TestMethod]
        public void SetPlainTextValue_ValidConfiguration_YieldsMatchingString()
        {
            var testPairs = new Dictionary<string, string>()
            {
                { "PROPERTY", "This is a plain-text property." },
                { "NESTED_PROPERTY:LEVEL1", "This a nested plain-text property." },
                { "NESTED_PROPERTY:LEVEL2:LEVEL2", "This is a twice-nested plain text property." }
            };

            var config = GetPlainTextConfiguration();

            foreach (var keypair in testPairs)
            {
                config[keypair.Key] = keypair.Value;
            }
            config.Commit();

            var updatedConfig = GetPlainTextConfiguration();

            foreach (var keypair in testPairs)
            {
                Debug.WriteLine($"Key: {keypair.Key}\nExpected: {keypair.Value}\nObserved: {updatedConfig[keypair.Key]}");
                Assert.AreEqual(expected: keypair.Value, actual: updatedConfig[keypair.Key]);
            }
        }

        /// <summary>
        /// Verifies setting an encrypted value is committed to file and accessible 
        /// to subsequent loads.
        /// </summary>
        [TestMethod]
        public void SetEncryptedValue_ValidConfiguration_YieldsMatchingString()
        {
            var testPairs = new Dictionary<string, string>()
            {
                { "PROPERTY", "This is a plain-text property." },
                { "NESTED_PROPERTY:LEVEL1", "This a nested plain-text property." },
                { "NESTED_PROPERTY:LEVEL2:LEVEL2", "This is a twice-nested plain text property." }
            };

            var config = GetEncryptedConfiguration();

            foreach(var keypair in testPairs)
            {
                config[keypair.Key] = keypair.Value;
            }
            config.Commit();

            var updatedConfig = GetEncryptedConfiguration();

            foreach(var keypair in testPairs)
            {
                Debug.WriteLine($"Key: {keypair.Key}\nExpected: {keypair.Value}\nObserved: {updatedConfig[keypair.Key]}");
                Assert.AreEqual(expected: keypair.Value, actual: updatedConfig[keypair.Key]);
            }
        }

        [TestMethod]
        public void RotateKey_ValidConfiguration_YieldsTrue()
        {
            var config = GetEncryptedConfiguration();
            config["Property"] = "Test";
            config.Commit();

            config.RotateKey("Configuration.Extensions.MSTest{2.0}");

            Assert.AreEqual(expected: "Test", actual: config["Property"]);

            var updatedConfig = GetAlternateEncryptedConfiguration();

            Assert.AreEqual(expected: "Test", actual: updatedConfig["Property"]);
        }

        private static IConfigurationRoot GetPlainTextConfiguration()
        {
            return new ConfigurationBuilder()
                    .AddJsonWritable(
                        path: "appsettings.plaintext.json",
                        optional: false,
                        reloadOnChange: true)
                    .Build();
        }

        private static IConfigurationRoot GetEncryptedConfiguration()
        {
            return new ConfigurationBuilder()
                    .AddSecureJsonWritable(
                        path: "appsettings.ciphertext.json",
                        optional: false,
                        reloadOnChange: true,
                        encryptionKeyContainer: "Configuration.Extensions.MSTest{1.0}",
                        logger: MSTest.Logger)
                    .Build();
        }

        private static IConfigurationRoot GetAlternateEncryptedConfiguration()
        
        {
            return new ConfigurationBuilder()
                    .AddSecureJsonWritable(
                        path: "appsettings.ciphertext.json",
                        optional: false,
                        reloadOnChange: true,
                        encryptionKeyContainer: "Configuration.Extensions.MSTest{2.0}",
                        logger: MSTest.Logger)
                    .Build();
        }
    }
}
