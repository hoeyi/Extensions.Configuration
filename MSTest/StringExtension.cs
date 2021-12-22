using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hoeyi.Configuration.Extensions;

namespace MSTest
{
    [TestClass]
    public class StringExtension
    {
        /// <summary>
        /// Verifies the output of <see cref="Hoeyi.Configuration.Extensions.StringExtension.ConvertToLogTemplate(string, string[])"/> 
        /// when no parameter is provided to a valid template.
        /// </summary>
        [TestMethod]
        public void ConvertToLogTemplate_ParameterlessTemplate_YieldsOriginalString()
        {
            string originalTemplate = "This is a parameterless template.";

            var convertedTemplate = originalTemplate.ConvertToLogTemplate();

            Assert.AreEqual(expected: originalTemplate, actual: convertedTemplate);
        }

        /// <summary>
        /// Verifies the output of <see cref="Hoeyi.Configuration.Extensions.StringExtension.ConvertToLogTemplate(string, string[])"/> 
        /// when a single parameter is provided to a valid template.
        /// </summary>
        [TestMethod]
        public void ConvertToLogTemplate_SingleParameterTemplate_YieldsConvertedString()
        {
            string originalTemplate = "This template has {0} parameter(s).";
            string convertedTemplate = originalTemplate.ConvertToLogTemplate("count");

            Assert.AreEqual(expected: "This template has {count} parameter(s).", actual: convertedTemplate);
        }

        /// <summary>
        /// Verifies the output of <see cref="Hoeyi.Configuration.Extensions.StringExtension.ConvertToLogTemplate(string, string[])"/> 
        /// when multiples parameters are provided to a valid template.
        /// </summary>
        [TestMethod]
        public void ConvertToLogTemplate_MultiParameterTemplate_YieldsConvertedString()
        {
            string originalTemplate = "This {0} has {1} parameter(s).";
            string convertedTemplate = originalTemplate.ConvertToLogTemplate("template", "count");

            Assert.AreEqual(expected: "This {template} has {count} parameter(s).", actual: convertedTemplate);
        }
    }
}
