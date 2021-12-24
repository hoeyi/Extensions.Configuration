using System;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Hoeyi.Extensions.Configuration.Resources;

namespace Hoeyi.Extensions.Configuration.Cryptography
{
    /// <summary>
    /// Represents a means to access an RSA public/private key pair for encrypting/decrypting values.
    /// </summary>
    sealed partial class RSAKeyStore
    {
        private readonly ILogger logger;
        private readonly CspParameters cspParams;
        private readonly Encoding byteConverter;

        /// <summary>
        /// Creates a new instance of <see cref="RSAKeyStore"/> using the given 
        /// key container name.
        /// </summary>
        /// <param name="keyContainerName">The RSA key container to use for asymmetric encryption.</param>
        /// <param name="logger">A <see cref="ILogger"/>.</param>
        public RSAKeyStore(string keyContainerName, ILogger logger)
        {
            if(!OperatingSystem.IsWindows())
                throw new NotSupportedException($"{Environment.OSVersion}");

            this.logger = @logger;

            cspParams = new CspParameters()
            {
                KeyContainerName = keyContainerName,
                Flags = CspProviderFlags.NoPrompt | CspProviderFlags.UseExistingKey
            };

            byteConverter = new UTF8Encoding();

            if(!KeyExists(keyContainerName))
                CreateKeyInContainer(keyContainerName: keyContainerName);
        }

        /// <summary>
        /// Gets the <see cref="CspParameters.KeyContainerName"/> for the underlying 
        /// <see cref="CspParameters"/>.
        /// </summary>
        public string KeyContainerName 
        {
            get
            {
                if (OperatingSystem.IsWindows())
                    return cspParams.KeyContainerName;
                else
                    return null;
            }
        }

        /// <summary>
        /// Encrypts the given text using unicode encoding.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <returns>The ciphertext from <paramref name="plainText"/>.</returns>
        public string Encrypt(string plainText)
        {
            if (!OperatingSystem.IsWindows())
                return null;

            try
            {
                var bytes = byteConverter.GetBytes(plainText);
                using var rsa = new RSACryptoServiceProvider(cspParams);
                
                var encryptedBytes = rsa.Encrypt(rgb: bytes, fOAEP: true);

                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception e)
            {
                logger.LogError(e, ExceptionString.KeyStore_EncryptionFailed);
                throw;
            }

        }

        /// <summary>
        /// Decrypts the given ciphertext.
        /// </summary>
        /// <param name="ciphterText64">The ciphertext to decrypt.</param>
        /// <returns>The plain text from <paramref name="ciphterText64"/>.</returns>
        public string Decrypt(string ciphterText64)
        {
            if (!OperatingSystem.IsWindows())
                return null;

            try
            {
                var bytes = Convert.FromBase64String(ciphterText64);
                using var rsa = new RSACryptoServiceProvider(cspParams);

                var decryptedBytes = rsa.Decrypt(rgb: bytes, fOAEP: true);

                return byteConverter.GetString(decryptedBytes);
            }
            catch (Exception e)
            {
                logger.LogError(e, ExceptionString.KeyStore_DecryptionFailed);
                throw;
            }
        }

        /// <summary>
        /// Deletes the key container represented by this <see cref="RSAKeyStore"/>.
        /// </summary>
        /// <returns>True if the operation is successful, else false.</returns>
        public bool DeleteKeyContainer()
        {
            if (!OperatingSystem.IsWindows())
                return true;

            return DeleteKeyFromContainer(cspParams.KeyContainerName);
        }

        /// <summary>
        /// Creates a key entry with the given container name.
        /// </summary>
        /// <param name="keyContainerName"></param>
        /// <returns>True if the operation is successful, else false.</returns>
        private bool CreateKeyInContainer(string keyContainerName)
        {
            if (!OperatingSystem.IsWindows())
                return false;

            var cspParams = new CspParameters()
            {
                KeyContainerName = keyContainerName
            };
            try
            {
                // Create a new instance of RSACryptoServiceProvider to save key in container.
                using var rsa = new RSACryptoServiceProvider(cspParams)
                {
                    PersistKeyInCsp = true,
                    KeySize = 4096
                };

                Debug.WriteLine($"{cspParams.KeyContainerName} created.");

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.Message}");
                DEBUG_WriteCspParameters(cspParams);
                logger.LogError(e,
                    ExceptionString.KeyStore_CreateKeyFailed
                        .ConvertToLogTemplate("KeyContainerName", "KeyProviderName"),
                    cspParams.KeyContainerName, cspParams.ProviderName);
                throw;
            }
        }

        /// <summary>
        /// Deletes the key corresonding to the given container name from the RSA key store./>
        /// </summary>
        /// <param name="containerName">The name of the key container to delete.</param>
        /// <returns>True if the operation is successful, else false.</returns>
        private bool DeleteKeyFromContainer(string containerName)
        {
            if (!OperatingSystem.IsWindows())
                return false;

            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullException(paramName: nameof(containerName));

            if (!KeyExists(containerName))
                return true;

            var cspParams = new CspParameters()
            {
                KeyContainerName = containerName
            };
            try
            {


                // Create a new instance of RSACryptoServiceProvider that accesses
                // the key container.
                using var rsa = new RSACryptoServiceProvider(cspParams)
                {
                    // Delete the key entry in the container.
                    PersistKeyInCsp = false
                };

                // Present in example code in Microsoft docs, but not necessary. Calling 
                // rsa.Clear() causes null reference exception due to elimination of rsa variable.
                //// Call Clear to release resources and delete the key from the container.

                Debug.WriteLine($"{cspParams.KeyContainerName} deleted.");

                return true;
            }
            catch(Exception e)
            {
                Debug.Write($"{e}");
                logger.LogError(e,
                    ExceptionString.KeyStore_DeleteKeyFailed
                        .ConvertToLogTemplate("KeyContainerName", "KeyProviderName"),
                    cspParams.KeyContainerName, cspParams.ProviderName);
                return false;
            }
        }

        /// <summary>
        /// Checks to see if the given key container exists.
        /// </summary>
        /// <param name="containerName">The name of the key container sought.</param>
        /// <returns>True if the key container exists, else false.</returns>
        private static bool KeyExists(string containerName)
        {
            if (!OperatingSystem.IsWindows())
                return false;

            try
            {
                var cspParams = new CspParameters()
                {
                    KeyContainerName = containerName,
                    Flags = CspProviderFlags.UseExistingKey
                };

                using var rsa = new RSACryptoServiceProvider(cspParams);

                return true;
            }
            catch(Exception e)
            {
                Debug.WriteLine($"{e.Message}");
                return false;
            }
        }
    }

    sealed partial class RSAKeyStore
    {
        [Conditional("DEBUG")]
        private static void DEBUG_WriteCspParameters(CspParameters cspParams)
        {
            if (cspParams is null)
                return;

            if (OperatingSystem.IsWindows())
            {
                #pragma warning disable CA1416 // Validate platform compatibility

                Debug.Write($"Params:\n[\n" +
                    $"\n\t{nameof(CspParameters.ProviderType)}: {cspParams.ProviderType}" +
                    $"\n\t{nameof(CspParameters.KeyContainerName)}: {cspParams.KeyContainerName}" +
                    $"\n\t{nameof(CspParameters.Flags)}: {cspParams.Flags}" +
                    $"\n]\n");

                #pragma warning restore CA1416 // Validate platform compatibility
            }
            else
                return;
        }

        [Conditional("DEBUG")]
#pragma warning disable IDE0051 // Remove unused private members
        private static void DEBUG_WriteRsaProviderToXML(RSA rsa)
#pragma warning restore IDE0051 // Remove unused private members
        {
            if (rsa is null)
                return;

            string xmlString = rsa.ToXmlString(includePrivateParameters: true);
            XDocument doc = XDocument.Parse(xmlString);

            Debug.Write($"\n{doc}\n");
        }
    }
}
