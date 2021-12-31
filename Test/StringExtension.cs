using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hoeyi.Extensions.Shared;

namespace Extensions.Configuration.Test
{
    [TestClass]
    public class StringExtension
    {
        [TestMethod]
        public void ConvertToLogTemplate_ParameterlessTemplate_YieldsOriginalString()
        {
            string originalTemplate = "This is a parameterless template.";
            var convertedTemplate = originalTemplate.ConvertToLogTemplate();

            Reference.Logger.LogDebug("Original template: {OriginalTemplate}", originalTemplate);

            Assert.AreEqual(expected: originalTemplate, actual: convertedTemplate);
        }

        [TestMethod]
        public void ConvertToLogTemplate_SingleParameterTemplate_YieldsConvertedString()
        {
            string originalTemplate = "This template has {0} parameter(s).";
            string convertedTemplate = originalTemplate.ConvertToLogTemplate("count");

            Reference.Logger.LogDebug("Original template: {OriginalTemplate}", originalTemplate);
            Reference.Logger.LogDebug("Converted template: {ConvertedTemplate}", convertedTemplate);

            Assert.AreEqual(expected: "This template has {count} parameter(s).", actual: convertedTemplate);
        }

        [TestMethod]
        public void ConvertToLogTemplate_MultiParameterTemplate_YieldsConvertedString()
        {
            string originalTemplate = "This {0} has {1} parameter(s).";
            string convertedTemplate = originalTemplate.ConvertToLogTemplate("template", "count");

            Reference.Logger.LogDebug("Original template: {OriginalTemplate}", originalTemplate);
            Reference.Logger.LogDebug("Converted template: {ConvertedTemplate}", convertedTemplate);

            Assert.AreEqual(expected: "This {template} has {count} parameter(s).", actual: convertedTemplate);
        }
    }
}
