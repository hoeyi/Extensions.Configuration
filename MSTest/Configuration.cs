using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ichsoft.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using System;

namespace MSTest
{
    [TestClass]
    public class Configuration
    {
        [TestMethod]
        public void InitConfiguration_ValidFile_YieldsInstance()
        {
            var c = new ConfigurationBuilder()
                .AddJsonWritable(
                    path: "appsettings.plaintext.json",
                    optional: false,
                    reloadOnChange: true)
                .Build();

            Assert.IsInstanceOfType(c, typeof(IConfigurationRoot));
        }

        [TestMethod]
        public void InitConfigurationEncrypted_ValidFile_YieldsInstance()
        {

        }

        [TestMethod]
        public void SetPlainTextValue_ValidConfiguration_YieldsTrue()
        {
            //configRoot["ConnectionStrings:PlainText"] = "MS test unencrypted string.";

            //configRoot.Commit();
        }

        [TestMethod]
        public void SetEncryptedValue_ValidConfiguration_YieldsTrue()
        {
            //configRoot["ConnectionStrings:Test"] = "MS test connection string.";

            //configRoot.Commit();
        }
    }
}
