using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hoeyi.Extensions.Configuration.Cryptography;
using Hoeyi.Extensions.Configuration.UnitTest.Setup;
using Hoeyi.Extensions.Configuration.UnitTest.Resources;

namespace Hoeyi.Extensions.Configuration.UnitTest
{
    [TestClass]
    public class SymmetricEncryption
    {
        [TestMethod]
        public void AesEncrypt_InputString_YieldsString()
        {
            var plainText = "Encrypt this string";

            string aesKey = AesWorker.GenerateKey(keySize: 256);

            var cipherText = AesWorker.Encrypt(
                plainText: plainText,
                aesKey: aesKey,
                out string aesIV);

            ResultCode resultCode;
            try
            {
                Global.Logger.LogInformation(InformationString.ResultInfo_Encrypt,
                    EntryType.RESULTINFO,
                    MethodBase.GetCurrentMethod().Name,
                    plainText, 
                    cipherText,
                    aesIV);

                Assert.IsInstanceOfType(cipherText, typeof(string));
                Assert.IsInstanceOfType(aesIV, typeof(string));

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
        public void AesEncryptDecrypt_InputString_YieldsOriginalString()
        {
            var plainText = "Encrypt this string";

            string aesKey = AesWorker.GenerateKey(keySize: 256);

            var cipherText = AesWorker.Encrypt(
                plainText: plainText,
                aesKey: aesKey,
                out string aesIV);

            var decryptedText = AesWorker.Decrypt(
                cipherText: cipherText,
                aesKey: aesKey,
                aesIV: aesIV);
            
            ResultCode resultCode;
            try
            {
                Global.Logger.LogInformation(InformationString.ResultInfo_EncryptDecrypt,
                    EntryType.RESULTINFO,
                    MethodBase.GetCurrentMethod().Name,
                    plainText,
                    cipherText,
                    aesIV,
                    decryptedText);

                Assert.AreEqual(plainText, decryptedText);

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
        public void AesEncrypt_InputString_MultipleCalls_YieldsDifferentCipherAndIV()
        {
            var plainText = "Encrypt this string";

            string aesKey = AesWorker.GenerateKey(keySize: 256);

            var cipher1 = AesWorker.Encrypt(
                plainText: plainText,
                aesKey: aesKey,
                out string iv1);

            var cipher2 = AesWorker.Encrypt(
                plainText: plainText,
                aesKey: aesKey,
                out string iv2);

            ResultCode resultCode;
            try
            {
                Assert.AreNotEqual(cipher1, cipher2);
                Assert.AreNotEqual(iv1, iv2);

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
