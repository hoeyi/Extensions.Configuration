using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hoeyi.Extensions.Configuration.UnitTest.Setup;
using Hoeyi.Extensions.Configuration.UnitTest.Resources;

namespace Hoeyi.Extensions.Configuration.UnitTest
{
    [TestClass]
    public class JsonSecureWritableConfigurationProvider
    {
        private const string encryptionKeyParameter = "_file:AesKeyCipher";
        private readonly IReadOnlyDictionary<string, string> testPairs =
            new Dictionary<string, string>()
            {
                    { "PROPERTY", "This is a plain-text property." },
                    { "NESTED_PROPERTY:LEVEL1", "This a nested plain-text property." },
                    { "NESTED_PROPERTY:LEVEL2:LEVEL1", "This is a twice-nested plain text property." }
            };

        [TestMethod]
        public void InitializeProvider_ValidFile_YieldsInstance()
        {
            var config = BuildConfiguration();

            ResultCode resultCode;
            try
            {
                Assert.IsInstanceOfType(config, typeof(IConfigurationRoot));

                resultCode = ResultCode.PASSED;
                Global.Logger.LogInformation(InformationString.Result_General,
                    EntryType.RESULT, MethodBase.GetCurrentMethod().Name, resultCode);
            }
            catch(UnitTestAssertException)
            {
                resultCode = ResultCode.FAILED;
                Global.Logger.LogInformation(InformationString.Result_General, 
                    EntryType.RESULT, MethodBase.GetCurrentMethod().Name, resultCode);
                throw;
            }

            ResetConfiguration();
        }

        [TestMethod]
        public void RotateKey_SubsequentLoad_YieldsMatchingString()
        {
            var config = BuildConfiguration();
            string testValue = "Test secured value.";
            string testPropertyKey = "PROPERTY";
            config[testPropertyKey] = testValue;
            config.Commit();

            config.RotateKey($"{Global.AssemblyName}.v2");
            var updatedConfig = BuildConfiguration(defaultKey: false);

            ResultCode resultCode;
            try
            {
                // Check the original configuration can decypher the value.
                Assert.AreEqual(expected: testValue, actual: config[testPropertyKey]);

                // Assert a new configuration built from the source file can 
                // decypher the value.
                Assert.AreEqual(expected: testValue, actual: updatedConfig[testPropertyKey]);

                resultCode = ResultCode.PASSED;
                Global.Logger.LogInformation(InformationString.Result_General,
                    EntryType.RESULT, MethodBase.GetCurrentMethod().Name, resultCode);
            }
            catch (UnitTestAssertException)
            {
                resultCode = ResultCode.FAILED;
                Global.Logger.LogInformation(InformationString.Result_General,
                    EntryType.RESULT, MethodBase.GetCurrentMethod().Name, resultCode);
                throw;
            }

            ResetConfiguration();
        }

        [TestMethod]
        public void Setvalues_SubsequentLoad_YieldsMatchingString()
        {
            var config = BuildConfiguration();

            foreach (var keypair in testPairs)
            {
                config[keypair.Key] = keypair.Value;
            }
            config.Commit();

            var updatedConfig = BuildConfiguration();

            var key1 = config[encryptionKeyParameter];
            var key2 = updatedConfig[encryptionKeyParameter];

            ResultCode resultCode;
            try
            {
                // Assert secret key is not revealed.
                Assert.IsTrue(key1 is null && key2 is null);

                foreach (var keypair in testPairs.Where(kp => kp.Key != encryptionKeyParameter))
                {
                    Global.Logger.LogInformation(InformationString.ResultInfo_KeyPair,
                        EntryType.RESULTINFO,
                        MethodBase.GetCurrentMethod().Name, 
                        keypair.Key, 
                        keypair.Value, 
                        updatedConfig[keypair.Key]);

                    // Assert each key pair from the original matches the corresponding 
                    // value from the updated configuration.
                    Assert.AreEqual(expected: keypair.Value, actual: updatedConfig[keypair.Key]);
                }

                resultCode = ResultCode.PASSED;
                Global.Logger.LogInformation(InformationString.Result_General,
                    EntryType.RESULT, MethodBase.GetCurrentMethod().Name, resultCode);
            }
            catch (UnitTestAssertException)
            {
                resultCode = ResultCode.FAILED;
                Global.Logger.LogInformation(InformationString.Result_General,
                    EntryType.RESULT, MethodBase.GetCurrentMethod().Name, resultCode);
                throw;
            }

            ResetConfiguration();
        }

        private static IConfigurationRoot BuildConfiguration(bool defaultKey = true)
        {
            string name = Global.AssemblyName;
            string version = defaultKey ? "v1" : "v2";
            string keyContainerName = $"{name}.{version}";

            var config = new ConfigurationBuilder()
                    .AddSecureJsonWritable(
                        path: $"appsettings.ciphertext.json",
                        optional: false,
                        reloadOnChange: true,
                        logger: Global.Logger)
                    .Build();

            if (config["_file:RsaKeyContainer"] is null)
                config["_file:RsaKeyContainer"] = $"{name}.{version}";

            return config;
        }

        private static void ResetConfiguration()
        {
            //return;
            File.WriteAllText("appsettings.ciphertext.json", "{\n}");
            Global.Logger.LogInformation(InformationString.Entry_General, 
                EntryType.ACTION,
                "Encrypted configuration file reset to default.");
        }
    }
}
