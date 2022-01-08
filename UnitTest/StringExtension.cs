using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ichosoft.Extensions.Configuration.UnitTest.Setup;
using Ichosoft.Extensions.Configuration.UnitTest.Resources;
using Hoeyi.Extensions.Shared;

namespace Ichosoft.Extensions.Configuration.UnitTest
{
    [TestClass]
    public class StringExtension
    {
        [TestMethod]
        public void ConvertToLogTemplate_ParameterlessTemplate_YieldsOriginalString()
        {
            string expected = "This is a parameterless template.";
            string observed = expected.ConvertToLogTemplate();

            ResultCode resultCode;
            try
            {
                Global.Logger.LogInformation(InformationString.ResultInfo_Comparison_SingleVariable,
                    EntryType.RESULTINFO,
                    MethodBase.GetCurrentMethod().Name,
                    expected,
                    observed);

                Assert.AreEqual(expected: expected, actual: observed);

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
        public void ConvertToLogTemplate_SingleParameterTemplate_YieldsConvertedString()
        {
            string expected = "This template has {count} parameter(s).";
            string observed = "This template has {0} parameter(s).".ConvertToLogTemplate("count");

            ResultCode resultCode;
            try
            {
                Global.Logger.LogInformation(InformationString.ResultInfo_Comparison_SingleVariable,
                    EntryType.RESULTINFO,
                    MethodBase.GetCurrentMethod().Name,
                    expected,
                    observed);

                Assert.AreEqual(expected: expected, actual: observed);

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
        public void ConvertToLogTemplate_MultiParameterTemplate_YieldsConvertedString()
        {
            string expected = "This {template} has {count} parameter(s).";
            string observed = "This {0} has {1} parameter(s).".ConvertToLogTemplate("template", "count");
            
            ResultCode resultCode;
            try
            {
                Global.Logger.LogInformation(InformationString.ResultInfo_Comparison_SingleVariable,
                    EntryType.RESULTINFO,
                    MethodBase.GetCurrentMethod().Name,
                    expected,
                    observed);

                Assert.AreEqual(expected: expected, actual: observed);

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
    }
}
