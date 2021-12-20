using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using Ichsoft.Configuration.Extensions.Cryptography;

namespace Ichsoft.Configuration.Extensions
{
    /// <summary>
    /// A JSON file configuration provider derived from <see cref="JsonConfigurationProvider"/>,
    /// which allows for committing in-memory changes to the source file.
    /// </summary>
    class JsonWritableConfigurationProvider : JsonConfigurationProvider, IConfigurationProvider, IWritableConfigurationProvider
    {
        private readonly RSAKeyStore rsaKeyStore;
        private readonly bool useEncryption;
        public JsonWritableConfigurationProvider(JsonWritableConfigurationSource source) : base(source)
        {
        }

        public JsonWritableConfigurationProvider(JsonConfigurationSource source, RSAKeyStore keyStore) : base(source)
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

        public void Commit()
        {
            IFileInfo fi = Source.FileProvider.GetFileInfo(Source.Path);

            // Convert the provider configuration data to JSON
            string jsonConfig = ToJson(Data);

            // Write all values to source file
            File.WriteAllText(fi.PhysicalPath, jsonConfig);
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
