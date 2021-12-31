using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hoeyi.Extensions.Configuration;

namespace Extensions.Configuration.Test
{
    [TestClass]
    public class JsonSecureWritableConfigurationProvider
    {
        [TestMethod]
        public void InitializeProvider_ValidFile_YieldsInstance()
        {
            var config = BuildConfiguration();

            Assert.IsInstanceOfType(config, typeof(IConfigurationRoot));
        }

        [TestMethod]
        public void Setvalue_Subsequent_YieldsMatchingString()
        {
            var testPairs = new Dictionary<string, string>()
            {
                { "PROPERTY", "This is a plain-text property." },
                { "NESTED_PROPERTY:LEVEL1", "This a nested plain-text property." },
                { "NESTED_PROPERTY:LEVEL2:LEVEL2", "This is a twice-nested plain text property." }
            };

            var config = BuildConfiguration();

            foreach (var keypair in testPairs)
            {
                config[keypair.Key] = keypair.Value;
            }
            config.Commit();

            var updatedConfig = BuildConfiguration();

            foreach (var keypair in testPairs)
            {
                Reference.Logger.LogDebug(
                    $"Key: {keypair.Key}\nExpected: {keypair.Value}\nObserved: {updatedConfig[keypair.Key]}");
                Assert.AreEqual(expected: keypair.Value, actual: updatedConfig[keypair.Key]);
            }
        }

        [TestMethod]
        public void RotateKey_SubsequentLoad_YieldsMatchingString()
        {
            var config = BuildConfiguration();
            config["Property"] = "Test";
            config.Commit();

            config.RotateKey($"{Reference.AssemblyName}.v2");

            Assert.AreEqual(expected: "Test", actual: config["Property"]);

            var updatedConfig = BuildConfiguration(defaultKey: false);

            Assert.AreEqual(expected: "Test", actual: updatedConfig["Property"]);
        }

        private static IConfigurationRoot BuildConfiguration(bool defaultKey = true)
        {
            string name = Reference.AssemblyName;
            string version = defaultKey ? "v1" : "v2";
            return new ConfigurationBuilder()
                    .AddSecureJsonWritable(
                        path: "appsettings.ciphertext.json",
                        optional: false,
                        reloadOnChange: true,
                        encryptionKeyContainer:$"{name}.{version}" ,
                        logger: Reference.Logger)
                    .Build();
        }
    }
}
