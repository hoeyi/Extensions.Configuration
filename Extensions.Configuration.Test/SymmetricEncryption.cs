using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ichosys.Extensions.Configuration.Cryptography;
using Ichosys.Extensions.Configuration.Test.Setup;
using Ichosys.Extensions.Configuration.Test.Resources;
using System;
using System.Text;

namespace Ichosys.Extensions.Configuration.Test
{
    [TestClass]
    public class SymmetricEncryption
    {
        [TestMethod]
        public void AesEncrypt_InputString_YieldsString()
        {
            var plainText = "Encrypt this string";

            string aesKey = AESProvider.GenerateKey(keySize: 256);

            var cipherText = AESProvider.Encrypt(
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

            string aesKey = AESProvider.GenerateKey(keySize: 256);

            var cipherText = AESProvider.Encrypt(
                plainText: plainText,
                aesKey: aesKey,
                out string aesIV);

            var decipherText = AESProvider.Decrypt(
                cipherText: cipherText,
                aesKey: aesKey,
                aesIV: aesIV);

            int byteCount = Encoding.Unicode.GetByteCount(plainText);

            ResultCode resultCode;
            try
            {
                Global.Logger.LogInformation(InformationString.ResultInfo_EncryptDecrypt,
                    EntryType.RESULTINFO,
                    MethodBase.GetCurrentMethod().Name,
                    plainText,
                    cipherText,
                    aesIV,
                    decipherText,
                    byteCount);

                Assert.AreEqual(plainText, decipherText);

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

            string aesKey = AESProvider.GenerateKey(keySize: 256);

            var cipher1 = AESProvider.Encrypt(
                plainText: plainText,
                aesKey: aesKey,
                out string iv1);

            var cipher2 = AESProvider.Encrypt(
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

        [TestMethod]
        public void AesEncrypt_InputStringLoremIpsum_YieldsOriginalString()
        {
            string aesKey = AESProvider.GenerateKey(keySize: 256);

            string plainText = Global.LoremIpsum(
                minimumWords: default,
                maximumWords: 100,
                minimumSentences: 1,
                maximumSentences: 10,
                paragraphCount: 2);

            string cipherText = AESProvider.Encrypt(plainText, aesKey, out string aesIV);
            string decipherText = AESProvider.Decrypt($"{aesIV}{cipherText}", aesKey);
            int byteCount = Encoding.Unicode.GetByteCount(plainText);

            ResultCode resultCode;
            try
            {
                Global.Logger.LogInformation(InformationString.ResultInfo_EncryptDecrypt,
                    EntryType.RESULTINFO,
                    MethodBase.GetCurrentMethod().Name,
                    plainText,
                    cipherText,
                    aesIV,
                    decipherText,
                    byteCount);

                Assert.AreEqual(plainText, decipherText);

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
