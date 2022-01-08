using System.Reflection;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ichosoft.Extensions.Configuration.UnitTest.Setup;
using Ichosoft.Extensions.Configuration.UnitTest.Resources;

namespace Ichosoft.Extensions.Configuration.UnitTest
{
    [TestClass]
    public class JsonWritableConfigurationProvider
    {
        private readonly IReadOnlyDictionary<string, string> testPairs =
            new Dictionary<string, string>()
            {
                { "PROPERTY", "This is a plain-text property." },
                { "NESTED_PROPERTY:LEVEL1", "This a nested plain-text property." },
                { "NESTED_PROPERTY:LEVEL2:LEVEL1", "This is a twice-nested plain text property." }
            };

        [TestMethod]
        public void IntializeProvider_ValidFileSource_YieldsInstance()
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
            catch (UnitTestAssertException)
            {
                resultCode = ResultCode.FAILED;
                Global.Logger.LogInformation(InformationString.Result_General,
                    EntryType.RESULT, MethodBase.GetCurrentMethod().Name, resultCode);
                throw;
            }
        }

        [TestMethod]
        public void SetValues_WithSubsequentLoad_YieldsMatchingString()
        {
            // Build the configuration and load with test values.
            var config = BuildConfiguration();
            foreach (var keypair in testPairs)
            {
                config[keypair.Key] = keypair.Value;
            }

            // Save the values to disk.
            config.Commit();

            // Reload the configuration.
            var updatedConfig = BuildConfiguration();

            ResultCode resultCode;
            try
            {
                foreach (var keypair in testPairs)
                {
                    Global.Logger.LogInformation(InformationString.ResultInfo_KeyPair,
                        EntryType.RESULTINFO,
                        MethodBase.GetCurrentMethod().Name,
                        keypair.Key,
                        keypair.Value,
                        updatedConfig[keypair.Key]);

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
