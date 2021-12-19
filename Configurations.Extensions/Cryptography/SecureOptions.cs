using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Security;
using System.Reflection;
using Ichsoft.Configuration.Extensions.Resources;

namespace Ichsoft.Configuration.Extensions.Cryptography
{
    class SecureOptions
    {
        private const string CertStoreName = "MY";
        private const StoreLocation CertStoreLocation = StoreLocation.CurrentUser;
        private const int KeySize = 4096;

        private readonly IConfiguration configuration;
        private readonly string rsaCertificateThumbprint;
        private readonly string appCommonName;
        public SecureOptions(IConfiguration configuration)
        {
            this.configuration = configuration;

            rsaCertificateThumbprint = configuration.GetSection("RSACertificateThumbprint")?.Value;
        }

        public string GetValue(string key)
        {
            var encryptedValue = configuration.GetSection(key).Value;

            using var rsaPrivateKey = GetRSAKey(rsaCertificateThumbprint, KeyType.Private);

            if (rsaPrivateKey is null)
            {
                throw new InvalidOperationException(string.Format(ExceptionString.SecureOptions_CouldNotRetrieveValue, key));
            }
            return Decrypt(rsaPrivateKey, encryptedValue);
        }

        public void SetValue(string key, string value)
        {
            using var rsaPublicKey = GetRSAKey(rsaCertificateThumbprint, KeyType.Public);

            if (rsaPublicKey is null)
            {
                throw new InvalidOperationException(string.Format(ExceptionString.SecureOptions_CouldNotRetrieveValue, key));
            }

            configuration[key] = Encrypt(rsaPublicKey, value);
        }

        private static string GetAppCommonName(Assembly assembly)
        {
            string title = assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
            string version = assembly.GetCustomAttribute<AssemblyVersionAttribute>().Version;

            return $"{title} {version}";
        }

        /// <summary>
        /// Lists the two expected key types for an asymmetric key pair.
        /// </summary>
        private enum KeyType
        {
            Private = 0,
            Public = 1
        }

        /// <summary>
        /// Gets an instance of the base RSA class using the public or private key
        /// linked to the provided certificate thumbprint.
        /// </summary>
        /// <remarks>Only searches the Current User certificate store for a match.</remarks>
        /// <param name="thumbprint"></param>
        /// <param name="keyType"></param>
        /// <returns></returns>
        /// <exception cref="CryptographicException">The store could not be opened as requested.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="ArgumentException">The store contains invalid values, or the KeyType is invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The requested certificate could not be found.</exception>
        private static RSA GetRSAKey(string thumbprint, KeyType keyType)
        {
            if (string.IsNullOrEmpty(thumbprint))
            {
                throw new ArgumentNullException(paramName: nameof(thumbprint));
            }
            X509Certificate2 certificate;
            using (var certStore = new X509Store(CertStoreName, CertStoreLocation))
            {
                certStore.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                try
                {
                    certificate = certStore.Certificates.Find(
                        X509FindType.FindByThumbprint,
                        thumbprint,
                        false)[0];
                }
                finally
                {
                    certStore?.Close();
                }
            }
            using (certificate)
            {
                return keyType switch
                {
                    KeyType.Public => certificate.GetRSAPublicKey(),
                    KeyType.Private => certificate.GetRSAPrivateKey(),
                    _ => throw new InvalidEnumArgumentException(
                        string.Format(ExceptionString.SecureOptions_InvalidEnumArgument, keyType, nameof(KeyType)))
                };
            }
        }

        /// <summary>
        /// Creates a new RSA certificate in the user's local store.
        /// </summary>
        /// <returns>The string thumbprint of the new certificate as a string.</returns>
        /// <exception cref="ArgumentException">The hash algorithm was null or empty or the certificate store
        /// contains invalid values.</exception>
        /// <exception cref="ArgumentNullException">The RSA key class was null.</exception>
        /// <exception cref="CryptographicException">An error occured during a step in the certificate creation process,
        /// or the key size passed the to RSA creator is not supported by the default implementation, or the 
        /// certificate store cannot be open for writing.</exception>
        /// <exception cref="InvalidOperationException">The certificate requested was created 
        /// with an object that does not accept a signing key.</exception>
        /// <exception cref="SecurityException">The caller did not have the required permission.</exception>
        private string CreateUserRSACertificate()
        {
            // Generate RSA algorithm with AppSettings key size
            RSA rsaProvider = RSA.Create(KeySize);

            // Build the certificate request.
            CertificateRequest certificateRequest = new(
                $"CN={appCommonName}",
                rsaProvider,
                HashAlgorithmName.SHA512,
                RSASignaturePadding.Pkcs1);

            // Set the effective date as 15 minutes before current time.
            DateTime effectiveDate = DateTime.Now.AddMinutes(-15);
            using X509Certificate2 certificate = certificateRequest.CreateSelfSigned(
                effectiveDate,
                effectiveDate.AddDays(365));

            // Get byte array from PFX certificate export.
            byte[] certBytes = certificate.Export(X509ContentType.Pfx);

            // Build new certificate while persisting public/private key set.
            using X509Certificate2 certificateWithKeys = new(
                certBytes,
                string.Empty,
                X509KeyStorageFlags.PersistKeySet);

            // Set bytes to null. No longer needed.
            certBytes = null;

            // Open the certificate store at current user location.
            using X509Store certificateStore = new(CertStoreName, CertStoreLocation);

            try
            {
                // Open store fore writing.
                certificateStore.Open(OpenFlags.ReadWrite);

                // Persist the complete certificate.
                certificateStore.Add(certificateWithKeys);

                // Close the certificate store.
                certificateStore.Close();
                return certificate.Thumbprint;
            }
            finally
            {
                // Do clean up.
                certificateStore?.Close();
                certificate?.Dispose();
                certificateWithKeys?.Dispose();
            }
        }

        /// <summary>
        /// Encrypts the given plain text string using the given
        /// RSA provider.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="plainText"></param>
        /// <returns>The encrypted string calculated by the given plain-text and RSA provider.</returns>
        public static string Encrypt(RSA provider, string plainText)
        {
            return Convert.ToBase64String(
                provider.Encrypt(Encoding.UTF8.GetBytes(plainText), RSAEncryptionPadding.OaepSHA512));
        }

        /// <summary>
        /// Decrypts the given base-64 string using the given
        /// RSA provider.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="encryptedText"></param>
        /// <returns>The plain-text string returned by decrypting the given text.</returns>
        public static string Decrypt(RSA provider, string encryptedText)
        {
            return Encoding.UTF8.GetString(
                provider.Decrypt(Convert.FromBase64String(encryptedText),
                RSAEncryptionPadding.OaepSHA512));
        }
    }
}
