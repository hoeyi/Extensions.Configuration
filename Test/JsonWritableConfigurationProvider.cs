using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hoeyi.Extensions.Configuration;

namespace Extensions.Configuration.Test
{
    [TestClass]
    public class JsonWritableConfigurationProvider
    {
        [TestMethod]
        public void IntializeProvider_ValidFileSource_YieldsInstance()
        {
            var config = BuildConfiguration();

            Assert.IsInstanceOfType(config, typeof(IConfigurationRoot));
        }

        [TestMethod]
        public void SetValue_SubsequentLoad_YieldsMatchingString()
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


        private static IConfigurationRoot BuildConfiguration()
        {
            return new ConfigurationBuilder()
                    .AddJsonWritable(
                        path: "appsettings.plaintext.json",
                        optional: false,
                        reloadOnChange: true)
                    .Build();
        }
    }
}
