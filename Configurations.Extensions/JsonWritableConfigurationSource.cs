using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;
using System;

namespace Ichsoft.Configuration.Extensions
{
    /// <summary>
    /// Represents a JSON file as an <see cref="IConfigurationSource"/>, for 
    /// use with <see cref="JsonWritableConfigurationProvider"/>.
    /// </summary>
    class JsonWritableConfigurationSource : JsonConfigurationSource
    {
        private readonly string keyContainerName;
        private readonly ILogger logger;
        public JsonWritableConfigurationSource()
            : base()
        {
        }

        public JsonWritableConfigurationSource(string keyContainerName, ILogger logger)
            : base()
        {
            if (string.IsNullOrEmpty(keyContainerName))
                throw new ArgumentNullException(paramName: nameof(keyContainerName));

            if (logger is null)
                throw new ArgumentNullException(paramName: nameof(logger));

            this.keyContainerName = keyContainerName;
            this.logger = logger;
        }
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileProvider ??= builder.GetFileProvider();
            if (string.IsNullOrEmpty(keyContainerName))
                return new JsonWritableConfigurationProvider(this);
            else
                return new JsonWritableConfigurationProvider(
                    this, new Cryptography.RSAKeyStore(keyContainerName, logger));
        }
    }
}
