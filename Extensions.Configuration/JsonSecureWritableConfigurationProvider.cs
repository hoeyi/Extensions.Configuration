using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Hoeyi.Extensions.Configuration.Cryptography;
using Hoeyi.Extensions.Configuration.Resources;

namespace Hoeyi.Extensions.Configuration
{
    class JsonSecureWritableConfigurationProvider : 
        JsonWritableConfigurationProvider, IWritableConfigurationProvider, IRSAProtectedConfigurationProvider
    {
        private RSAKeyStore rsaKeyStore;
        private readonly ILogger logger;
        private const string SecretKeyParameter = "Encryption:SecretKey";

        /// <summary>
        /// Creates a new <see cref="JsonSecureWritableConfigurationProvider"/> for values held 
        /// as encrypted values except when accessed.
        /// </summary>
        /// <param name="source">A <see cref="JsonWritableConfigurationSource"/>.</param>
        /// <param name="keyContainername">The RSA key container name.</param>
        /// <param name="logger">An <see cref="ILogger"/>.</param>
        public JsonSecureWritableConfigurationProvider(
            JsonWritableConfigurationSource source, string keyContainername, ILogger logger)
            : this(source: source, new RSAKeyStore(keyContainerName: keyContainername, logger: logger))
        {
            this.logger = logger;
        }

        /// <summary>
        /// Creates a new <see cref="JsonSecureWritableConfigurationProvider"/> for values 
        /// held as encrypted values except when accessed.
        /// </summary>
        /// <param name="source">A <see cref="JsonWritableConfigurationSource"/>.</param>
        /// <param name="keyStore">An <see cref="RSAKeyStore"/>.</param>
        private JsonSecureWritableConfigurationProvider(
            JsonWritableConfigurationSource source, RSAKeyStore keyStore)
            : base(source)
        {
            if (source is null)
                throw new ArgumentNullException(paramName: nameof(source));

            if (keyStore is null)
                throw new ArgumentNullException(paramName: nameof(keyStore));

            rsaKeyStore = keyStore;
        }

        /// <summary>
        /// Gets the name of the RSA key container used by this provider. 
        /// </summary>
        public string KeyContainerName
        {
            get { return rsaKeyStore.KeyContainerName; }
        }


        /// <summary>
        /// Deletes the current key attached to this <see cref="IRSAProtectedConfigurationProvider"/>.
        /// </summary>
        /// <returns>True if the operation is successful, else false.</returns>
        public bool DeleteKey()
        {
            return rsaKeyStore.DeleteKeyContainer();
        }

        /// <summary>
        /// Rotates the key used to encrypt the values in the <see cref="IRSAProtectedConfigurationProvider"/> object.
        /// </summary>
        /// <param name="newKeyContainer">The name of a new RSA key container.</param>
        /// <param name="deleteOnSuccess">True to delete the old key container, else false.</param>
        /// <returns>True if the operation is successful, else false.</returns>
        /// <exception cref="InvalidOperationException">The secret key for the provider is not set.</exception>
        public bool RotateKey(string newKeyContainer, bool deleteOnSuccess = true)
        {
            var newKeyStore = new RSAKeyStore(keyContainerName: newKeyContainer, logger: logger);

            var backupData = Data.ToDictionary(kv => kv.Key, kv => kv.Value);

            try
            {
                if (base.TryGet(SecretKeyParameter, out string encryptedCurrentKey))
                {
                    string encryptedNewKey = newKeyStore.Encrypt(AesWorker.GenerateKey());
                    foreach (var keypair in Data.Where(kp => kp.Key != SecretKeyParameter))
                    {
                        string text = AesWorker.Decrypt(keypair.Value, rsaKeyStore.Decrypt(encryptedCurrentKey));
                        string newCipherText = AesWorker.Encrypt(
                            plainText: text,
                            aesKey: newKeyStore.Decrypt(encryptedNewKey),
                            out string aesIV);

                        Data[keypair.Key] = $"{aesIV}{newCipherText}";
                    }

                    Data[SecretKeyParameter] = encryptedNewKey;
                    Commit();

                    if (!rsaKeyStore.DeleteKeyContainer())
                        throw new InvalidOperationException(message:
                            string.Format(ExceptionString.KeyStore_DeleteKeyFailed, rsaKeyStore.KeyContainerName));

                    rsaKeyStore = newKeyStore;

                    logger?.LogDebug(LogMessage.EncryptionProvider_RotateKeySucceeded);
                    return true;
                }
                else
                    throw new InvalidOperationException(
                        message: ExceptionString.EncryptionProvider_KeyNotSet);
            }
            catch (Exception e)
            {
                foreach (var keypair in backupData)
                {
                    Data[keypair.Key] = keypair.Value;
                }
                Commit();

                logger?.LogError(e, ExceptionString.EncryptionProvider_RotateKeyFailed);
                throw;
            }
        }

        public override void Set(string key, string value)
        {
            if (key == SecretKeyParameter)
                return;
            
            // If SecretKey has been set, generate and saved to disk.
            if(!Data.ContainsKey(SecretKeyParameter))
            {
                base.Set(
                    key: SecretKeyParameter,
                    value: rsaKeyStore.Encrypt(AesWorker.GenerateKey()));
                Commit();
            }
            SecureSet(key, value);
        }

        public override bool TryGet(string key, out string value)
        {
            value = null;
            if (TryGetSecure(key, out string _value))
            {
                value = _value;
                return true;
            }
            else
                return false;
        }

        private void SecureSet(string key, string value)
        {
            if (base.TryGet(SecretKeyParameter, out string encryptedAesKey))
            {
                string cipherText = AesWorker.Encrypt(
                    plainText: value,
                    aesKey: rsaKeyStore.Decrypt(encryptedAesKey),
                    out string aesIV);

                base.Set(key, $"{aesIV}{cipherText}");
            }
            else
                throw new InvalidOperationException(
                    message: ExceptionString.EncryptionProvider_KeyNotSet);
        }

        private bool TryGetSecure(string key, out string value)
        {
            value = null;
            if (base.TryGet(SecretKeyParameter, out string encryptedAesKey))
            {
                if (base.TryGet(key, out string encryptedValue))
                {
                    value = AesWorker.Decrypt(
                        cipherTextWithIV: encryptedValue,
                        aesKey: rsaKeyStore.Decrypt(encryptedAesKey));
                    return true;
                }
                else
                    return false;
            }
            else
                throw new InvalidOperationException(
                    message: ExceptionString.EncryptionProvider_KeyNotSet);
        }
    }
}
