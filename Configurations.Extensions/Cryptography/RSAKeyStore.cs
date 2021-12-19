using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Ichsoft.Configuration.Extensions.Resources;

[assembly: InternalsVisibleTo(assemblyName: "Configuration.Extensions.MSTest")]

namespace Ichsoft.Configuration.Extensions.Cryptography
{
    partial class RSAKeyStore
    {
        private readonly ILogger logger;
        private readonly CspParameters cspParams;
        private readonly Encoding byteConverter;
        public RSAKeyStore(string containerName, ILogger logger)
        {
            if(!OperatingSystem.IsWindows())
                throw new NotSupportedException($"{Environment.OSVersion}");

            this.logger = @logger;

            cspParams = new CspParameters()
            {
                KeyContainerName = containerName,
                Flags = CspProviderFlags.NoPrompt | CspProviderFlags.UseExistingKey
            };

            byteConverter = new UTF8Encoding();

            if(!KeyExists(containerName))
                CreateKeyInContainer(containerName: containerName);
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

        //public bool RotateKeys(string newContainerName)
        //{
        //    if (!OperatingSystem.IsWindows())
        //        return false;

        //    if (string.IsNullOrEmpty(newContainerName))
        //        throw new ArgumentNullException(paramName: nameof(newContainerName));

        //    if (newContainerName == cspParams.KeyContainerName)
        //        throw new ArgumentException(message: Resources.ExceptionString.KeyStore_DuplicateKeyName);


        //}

        /// <summary>
        /// Creates a key entry with the given container name.
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns><see cref="true"/> if the operation is successful, else <see cref="false"/>.</returns>
        private bool CreateKeyInContainer(string containerName)
        {
            if (!OperatingSystem.IsWindows())
                return false;

            try
            {
                var cspParams = new CspParameters()
                {
                    KeyContainerName = containerName
                };

                // Create a new instance of RSACryptoServiceProvider that accesses
                // the key container MyKeyContainerName.
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
                Debug.WriteLine($"{e}");
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
        /// <returns><see cref="true"/> if the operation is successful, else <see cref="false"/>.</returns>
        public bool DeleteKeyFromContainer(string containerName)
        {
            if (!OperatingSystem.IsWindows())
                return false;

            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullException(paramName: nameof(containerName));

            try
            {
                if (!KeyExists(containerName))
                    return true;

                var cspParams = new CspParameters()
                {
                    KeyContainerName = containerName
                };

                // Create a new instance of RSACryptoServiceProvider that accesses
                // the key container.
                using var rsa = new RSACryptoServiceProvider(cspParams)
                {
                    // Delete the key entry in the container.
                    PersistKeyInCsp = false
                };

                // Call Clear to release resources and delete the key from the container.

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
        /// <returns><see cref="true"/> if the key container exists, else <see cref="false"/>.</returns>
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

    partial class RSAKeyStore
    {
        [Conditional("DEBUG")]
        private static void DEBUG_WriteCspParameters(CspParameters cspParams)
        {
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
        private static void DEBUG_WriteRsaProviderToXML(RSA rsa)
        {
            string xmlString = rsa.ToXmlString(includePrivateParameters: true);
            XDocument doc = XDocument.Parse(xmlString);

            Debug.Write($"\n{doc}\n");
        }
    }
}
