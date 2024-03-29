﻿using System;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Ichosys.Extensions.Configuration.Resources;
using Ichosys.Extensions.Common.Logging;

namespace Ichosys.Extensions.Configuration.Cryptography
{
    /// <summary>
    /// Represents a means to access an RSA public/private key pair for encrypting/decrypting values.
    /// </summary>
    sealed partial class RSAProvider
    {
        private readonly ILogger logger;
        private readonly CspParameters cspParams;
        private readonly Encoding byteConverter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyContainerName">The name of the key container to use and/or create.</param>
        /// <exception cref="NotSupportedException">The running platform does not support this class.</exception>
        /// <exception cref="ArgumentNullException">A parameter provider is null or empty.</exception>
        public RSAProvider(string keyContainerName)
        {
            if (!OperatingSystem.IsWindows())
                throw new NotSupportedException(string.Format(
                    ExceptionString.RSAKeyStore_PlatformNotSupported, Environment.OSVersion));

            if (string.IsNullOrEmpty(keyContainerName))
                throw new ArgumentNullException(paramName: keyContainerName);

            cspParams = new CspParameters()
            {
                KeyContainerName = keyContainerName,
                Flags = CspProviderFlags.NoPrompt | CspProviderFlags.UseExistingKey
            };

            byteConverter = new UTF8Encoding();

            if (!KeyExists(cspParams.KeyContainerName))
                CreateKeyInContainer(cspParams.KeyContainerName);
        }

        /// <summary>
        /// Creates a new instance of <see cref="RSAProvider"/> using the given 
        /// key container name.
        /// </summary>
        /// <param name="keyContainerName">The RSA key container to use for asymmetric encryption.</param>
        /// <param name="logger">A <see cref="ILogger"/>.</param>
        public RSAProvider(string keyContainerName, ILogger logger)
            : this(keyContainerName: keyContainerName)
        {
            this.logger = logger;
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
                logger?.LogError(e, ExceptionString.EncryptionProvider_EncryptionFailed);
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
                logger?.LogError(e, ExceptionString.EncryptionProvider_DecryptionFailed);
                throw;
            }
        }

        /// <summary>
        /// Deletes the key container represented by this <see cref="RSAProvider"/>.
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

                return true;
            }
            catch (Exception e)
            {
                logger?.LogError(e,
                    ExceptionString.RSAKeyStore_CreateKeyContainerFailed
                        .ConvertToLogTemplate(nameof(KeyContainerName)),
                    cspParams.KeyContainerName);
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
                // Call Clear to release resources and delete the key from the container.
                // rsa.Clear()

                return true;
            }
            catch(Exception e)
            {
                logger?.LogError(e,
                    ExceptionString.RSAKeyStore_DeleteKeyContainerFailed
                        .ConvertToLogTemplate(nameof(KeyContainerName)),
                    cspParams.KeyContainerName);
                
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
            catch(Exception)
            {
                return false;
            }
        }
    }
}
