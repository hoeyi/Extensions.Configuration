using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using Hoeyi.Extensions.Configuration.Cryptography;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Hoeyi.Extensions.Configuration
{
    /// <summary>
    /// A JSON file configuration provider derived from <see cref="JsonConfigurationProvider"/>,
    /// which allows for committing in-memory changes to the source file.
    /// </summary>
    class JsonWritableConfigurationProvider : 
        JsonConfigurationProvider, IConfigurationProvider, IWritableConfigurationProvider, IRSAProtectedConfigurationProvider
    {
        private RSAKeyStore rsaKeyStore;
        private readonly bool useEncryption;
        private readonly ILogger logger;

        /// <summary>
        /// Creates a new <see cref="JsonWritableConfigurationProvider"/> for values 
        /// saved in plain-text.
        /// </summary>
        /// <param name="source">A <see cref="JsonWritableConfigurationSource"/>.</param>
        public JsonWritableConfigurationProvider(JsonWritableConfigurationSource source) : base(source)
        {
        }

        /// <summary>
        /// Creates a new <see cref="JsonWritableConfigurationProvider"/> for values held 
        /// as encrypted values except when accessed.
        /// </summary>
        /// <param name="source">A <see cref="JsonWritableConfigurationSource"/>.</param>
        /// <param name="keyContainername">The RSA key container name.</param>
        /// <param name="logger">An <see cref="ILogger"/>.</param>
        public JsonWritableConfigurationProvider(
            JsonWritableConfigurationSource source, string keyContainername, ILogger logger)
            : this(source: source, new RSAKeyStore(keyContainerName: keyContainername, logger: logger))
        {
            this.logger = logger;
        }

        /// <summary>
        /// Creates a new <see cref="JsonWritableConfigurationProvider"/> for values 
        /// held as encrypted values except when accessed.
        /// </summary>
        /// <param name="source">A <see cref="JsonWritableConfigurationSource"/>.</param>
        /// <param name="keyStore">An <see cref="RSAKeyStore"/>.</param>
        private JsonWritableConfigurationProvider(
            JsonWritableConfigurationSource source, RSAKeyStore keyStore) 
            : this(source)
        {
            if (source is null)
                throw new ArgumentNullException(paramName: nameof(source));

            if (keyStore is null)
                throw new ArgumentNullException(paramName: nameof(keyStore));

            rsaKeyStore = keyStore;
            useEncryption = true;
        }

        public override void Set(string key, string value)
        {
            if (useEncryption)
                base.Set(key, rsaKeyStore.Encrypt(plainText: value));
            else
                base.Set(key, value);
        }

        public override bool TryGet(string key, out string value)
        {
            value = null;
            if(base.TryGet(key, out string _val))
            {
                if(useEncryption)
                {
                    value = rsaKeyStore.Decrypt(_val);
                    return true;
                }
                else
                {
                    value = _val;
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Saves all in-memory configuration changes to the <see cref="IWritableConfigurationProvider"/> 
        /// data store.
        /// </summary>
        public void Commit()
        {
            IFileInfo fi = Source.FileProvider.GetFileInfo(Source.Path);

            // Convert the provider configuration data to JSON
            string jsonConfig = ToJson(Data);

            // Write all values to source file
            File.WriteAllText(fi.PhysicalPath, jsonConfig);
        }

        /// <summary>
        /// Rotates the key used to encrypt the values in the <see cref="IRSAProtectedConfigurationProvider"/> object.
        /// </summary>
        /// <param name="newKeyContainer">The name of a new RSA key container.</param>
        /// <param name="deleteOnSuccess">True to delete the old key container, else false.</param>
        /// <returns>True if the operation is successful, else false.</returns>        
        public bool RotateKey(string newKeyContainer, bool deleteOnSuccess = true)
        {
            var newKeyStore = new RSAKeyStore(keyContainerName: newKeyContainer, logger: logger);

            var backupData = Data.ToDictionary(kv => kv.Key, kv => kv.Value);
            try
            {
                foreach(var keypair in Data)
                {
                    string text = rsaKeyStore.Decrypt(keypair.Value);
                    string newCipher = newKeyStore.Encrypt(text);

                    Debug.WriteLine($"Key: {keypair.Key}\nValue: {text}\nCipher: {keypair.Value}");
                    Data[keypair.Key] = newKeyStore.Encrypt(plainText: rsaKeyStore.Decrypt(keypair.Value));
                }

                Commit();

                if (!rsaKeyStore.DeleteKeyContainer())
                    throw new InvalidOperationException(message:
                        string.Format(Resources.ExceptionString.KeyStore_DeleteKeyFailed, rsaKeyStore.KeyContainerName));

                rsaKeyStore = newKeyStore;

                return true;
            }
            catch(Exception)
            {
                foreach(var keypair in backupData)
                {
                    Data[keypair.Key] = keypair.Value;
                }
                Commit();

                return false;
            }
        }

        private static string ToJson(IDictionary<string, string> props)
        {
            IDictionary<string, object> json = new System.Dynamic.ExpandoObject();

            foreach (var prop in props.OrderByDescending(kv => kv.Key))
            {
                // Get the complete path for the value
                string path = prop.Key;

                // If there is no path move the next key-value pair
                if (string.IsNullOrWhiteSpace(path)) continue;

                // Break the path into its component pieces
                string[] keys = path.Split(':');

                // Get the property value
                string value = prop.Value;

                var cursor = json;

                // Loop through key and sub-keys
                for (int i = 0; i < keys.Length; i++)
                {
                    if (!cursor.TryGetValue(keys[i], out _))
                    {
                        cursor.Add(keys[i], new System.Dynamic.ExpandoObject());
                    }
                    if (i == keys.Length - 1)
                    {
                        cursor[keys[i]] = value;
                    }

                    cursor = cursor[keys[i]] as IDictionary<string, object>;
                }
            }

            // Serialize resulting object and return
            return JsonConvert.SerializeObject(json, Formatting.Indented);
        }
    }
}
