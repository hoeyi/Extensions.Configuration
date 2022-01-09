using System;
using System.Collections.Generic;
using System.Linq;
using Ichosoft.Extensions.Configuration.Cryptography;
using Ichosoft.Extensions.Configuration.Resources;
using Microsoft.Extensions.Logging;

namespace Ichosoft.Extensions.Configuration
{
    /// <summary>
    /// A JSON file configuration provider derived from <see cref="JsonWritableConfigurationProvider"/>,
    /// which encrypts configuration values in membory and at-rest.
    /// </summary>
    class JsonSecureWritableConfigurationProvider :
        JsonWritableConfigurationProvider, IWritableConfigurationProvider, IRSAProtectedConfigurationProvider
    {
        private RSAProvider rsaKeyStore;
        private readonly ILogger logger;
        private const string _AesKeyCipherAddress = "_file:AesKeyCipher";
        private const string _RsaKeyContainerAddress = "_file:RsaKeyContainer";

        private readonly IReadOnlyCollection<string> plainTextSettings = new string[]
        {
            _RsaKeyContainerAddress
        };

        /// <summary>
        /// Configuration setting keys that are kept private.
        /// Calls for the values of these keys should be ignored.
        /// </summary>
        private readonly IReadOnlyCollection<string> privateSettings = new string[]
        {
            _AesKeyCipherAddress
        };

        /// <summary>
        /// Settings whose values are protected by the RSA public/private key pair 
        /// represented by the setting at 
        /// </summary>
        private readonly IReadOnlyCollection<string> rsaProtectedSettings = new string[]
        {
            _AesKeyCipherAddress,
        };

        /// <summary>
        /// Configuration setting keys that are assignable only if null.
        /// </summary>
        private readonly IReadOnlyCollection<string> readonlyKeys = new string[]
        {
            _RsaKeyContainerAddress
        };

        /// <summary>
        /// Gets the keys for settings not protected by symmetric encryption.
        /// </summary>
        private IReadOnlyCollection<string> NonAesProtectedSettings
        {
            get
            {
                return rsaProtectedSettings.Concat(plainTextSettings).ToList();
            }
        }

        public JsonSecureWritableConfigurationProvider(
            JsonWritableConfigurationSource source, ILogger logger)
            : base(source)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Creates a new <see cref="JsonSecureWritableConfigurationProvider"/> for values held 
        /// as encrypted values except when accessed.
        /// </summary>
        /// <param name="source">A <see cref="JsonWritableConfigurationSource"/>.</param>
        /// <param name="keyContainerName">The RSA key container name.</param>
        /// <param name="logger">An <see cref="ILogger"/>.</param>
        public JsonSecureWritableConfigurationProvider(
            JsonWritableConfigurationSource source, string keyContainerName, ILogger logger)
            : this(source, new RSAProvider(keyContainerName, logger))
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
            JsonWritableConfigurationSource source, RSAProvider keyStore)
            : base(source)
        {
            if (source is null)
                throw new ArgumentNullException(paramName: nameof(source));

            if (keyStore is null)
                throw new ArgumentNullException(paramName: nameof(keyStore));

            rsaKeyStore = keyStore;
        }

        /// <summary>
        /// Gets the current <see cref="RSAKeyStore"/> for this provider. A new 
        /// instance is created if the value is null.
        /// </summary>
        private RSAProvider RSAKeyStore
        {
            get
            {
                if(!base.TryGet(_RsaKeyContainerAddress, out string keyContainer))
                    throw new InvalidOperationException(
                        string.Format(ExceptionString.EncryptionProvider_ProviderNotSet, nameof(RSAProvider)));

                rsaKeyStore ??= new RSAProvider(keyContainer, logger);
                return rsaKeyStore;
            }   
        }

        /// <summary>
        /// Gets the name of RSA key container for this provider.
        /// </summary>
        public string KeyContainerName
        {
            get{ return base.TryGet(_RsaKeyContainerAddress, out string @value) ? @value : null; }
        }

        /// <summary>
        /// Deletes the current key attached to this <see cref="IRSAProtectedConfigurationProvider"/>.
        /// </summary>
        /// <returns>True if the operation is successful, else false.</returns>
        public bool DeleteKey()
        {
            return RSAKeyStore.DeleteKeyContainer();
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
            var newKeyStore = new RSAProvider(keyContainerName: newKeyContainer, logger: logger);

            var backupData = Data.ToDictionary(kv => kv.Key, kv => kv.Value);

            try
            {
                if (base.TryGet(_AesKeyCipherAddress, out string encryptedCurrentKey))
                {
                    string encryptedNewKey = newKeyStore.Encrypt(AESProvider.GenerateKey());

                    foreach (var keypair in Data.Where(kp => !NonAesProtectedSettings.Contains(kp.Key)))
                    {
                        string text = AESProvider.Decrypt(keypair.Value, RSAKeyStore.Decrypt(encryptedCurrentKey));
                        string newCipherText = AESProvider.Encrypt(
                            plainText: text,
                            aesKey: newKeyStore.Decrypt(encryptedNewKey),
                            out string aesIV);

                        Data[keypair.Key] = $"{aesIV}{newCipherText}";
                    }

                    Data[_AesKeyCipherAddress] = encryptedNewKey;
                    Data[_RsaKeyContainerAddress] = newKeyStore.KeyContainerName;
                    Commit();

                    if (!RSAKeyStore.DeleteKeyContainer())
                        throw new InvalidOperationException(message:
                            string.Format(ExceptionString.RSAKeyStore_DeleteKeyContainerFailed, RSAKeyStore.KeyContainerName));

                    rsaKeyStore = newKeyStore;

                    return true;
                }
                else
                    throw new InvalidOperationException(
                        string.Format(ExceptionString.EncryptionProvider_ProviderNotSet, nameof(AESProvider)));
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
            
            if(readonlyKeys.Contains(key) && base.TryGet(key, out string _))
            {
                throw new InvalidOperationException(
                    string.Format(ExceptionString.EncryptionProvider_SettingIsReadOnly, key));
            }

            if(plainTextSettings.Contains(key))
            {
                base.Set(key, value);
                return;
            }

            if (rsaProtectedSettings.Contains(key))
            {
                SetRsaProtectedValue(key, value);
                return;
            }
            
            if (!SecretKeyInitialized())
                throw new InvalidOperationException();

            SetAesProtectedValue(key, value);
        }

        public override bool TryGet(string key, out string value)
        {
            value = null;

            if (privateSettings.Contains(key))
                return false;

            if(plainTextSettings.Contains(key))
            {
                if (base.TryGet(key, out string plainTextValue))
                {
                    value = plainTextValue;
                    return true;
                }
                else
                    return false;
            }

            if(rsaProtectedSettings.Contains(key))
            {
                if (TryGetRsaProtectedValue(key, out string plainTextValue))
                {
                    value = plainTextValue;
                    return true;
                }
                else
                    return false;
            }
              
            if (TryGetAesProtectedValue(key, out string _value))
            {
                value = _value;
                return true;
            }
            else
                return false;
        }

        private void SetAesProtectedValue(string key, string value)
        {
            if (base.TryGet(_AesKeyCipherAddress, out string encryptedAesKey))
            {
                string cipherText = AESProvider.Encrypt(
                    plainText: value,
                    aesKey: RSAKeyStore.Decrypt(encryptedAesKey),
                    out string aesIV);

                base.Set(key, $"{aesIV}{cipherText}");
            }
            else
                throw new InvalidOperationException(
                    string.Format(ExceptionString.EncryptionProvider_ProviderNotSet, nameof(AESProvider)));
        }

        private void SetRsaProtectedValue(string key, string value)
        {
            base.Set(key, RSAKeyStore.Encrypt(value));
        }

        private bool TryGetAesProtectedValue(string key, out string value)
        {
            value = null;
            if (base.TryGet(_AesKeyCipherAddress, out string encryptedAesKey))
            {
                if (base.TryGet(key, out string encryptedValue))
                {
                    value = AESProvider.Decrypt(
                        cipherTextWithIV: encryptedValue,
                        aesKey: RSAKeyStore.Decrypt(encryptedAesKey));
                    return true;
                }
                else
                    return false;
            }
            else
                throw new InvalidOperationException(
                    message: ExceptionString.EncryptionProvider_SymmetricProviderNotSet);
        }

        private bool TryGetRsaProtectedValue(string key, out string value)
        {
            value = null;
            if (base.TryGet(key, out string rsaProtectedValue))
            {
                value = RSAKeyStore.Decrypt(rsaProtectedValue);
                return true;
            }
            else
                return false;
        }

        private bool SecretKeyInitialized()
        {

            if(!base.TryGet(_AesKeyCipherAddress, out string encryptedAesKey) ||
                string.IsNullOrEmpty(encryptedAesKey))
            {
                base.Set(
                    key: _AesKeyCipherAddress, 
                    value: RSAKeyStore.Encrypt(AESProvider.GenerateKey()));
                Commit();
                return true;
            }

            return true;
        }
    }
}
